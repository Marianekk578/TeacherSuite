using FluentAssertions;
using FluentValidation;
using MediatR;
using TeacherSuite.Application.Common;
using ValidationException = TeacherSuite.Application.Common.ValidationException;

namespace Application.UnitTests.Behaviours;

public class ValidationBehaviourTests
{
    private readonly RequestHandlerDelegate<string> _next = (_) => Task.FromResult("Success");

    [Fact]
    public async Task Handle_WhenNoValidators_ShouldCallNext()
    {
        var behaviour = new ValidationBehaviour<TestRequest, string>(
            Enumerable.Empty<IValidator<TestRequest>>());

        var result = await behaviour.Handle(new TestRequest(), _next, CancellationToken.None);

        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WhenValidatorsPass_ShouldCallNext()
    {
        var validator = new InlineValidator<TestRequest>();
        var behaviour = new ValidationBehaviour<TestRequest, string>(new[] { validator });

        var result = await behaviour.Handle(new TestRequest(), _next, CancellationToken.None);

        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ShouldThrowValidationException()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Name).NotEmpty();

        var behaviour = new ValidationBehaviour<TestRequest, string>(new[] { validator });

        var act = () => behaviour.Handle(
            new TestRequest { Name = "" }, _next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenValidatorFails_ShouldContainFailureDetails()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");

        var behaviour = new ValidationBehaviour<TestRequest, string>(new[] { validator });

        var act = () => behaviour.Handle(
            new TestRequest { Name = "" }, _next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsFail_ShouldAggregateErrors()
    {
        var validator1 = new InlineValidator<TestRequest>();
        validator1.RuleFor(x => x.Name).NotEmpty();

        var validator2 = new InlineValidator<TestRequest>();
        validator2.RuleFor(x => x.Value).GreaterThan(0);

        var behaviour = new ValidationBehaviour<TestRequest, string>(
            new IValidator<TestRequest>[] { validator1, validator2 });

        var act = () => behaviour.Handle(
            new TestRequest { Name = "", Value = 0 }, _next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
    }

    public class TestRequest : IRequest<string>
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
