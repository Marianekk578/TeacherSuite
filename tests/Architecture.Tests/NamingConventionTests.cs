using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Architecture.Tests;

public class NamingConventionTests
{
    private static readonly System.Reflection.Assembly ApplicationAssembly =
        typeof(TeacherSuite.Application.Teachers.Commands.Create.CreateTeacherCommand).Assembly;

    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(ApplicationAssembly)
            .Build();

    [Fact]
    public void CommandHandlers_ShouldEndWith_CommandHandler()
    {
        Classes().That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().ResideInNamespaceMatching(@".*\.Commands(\..*)?")
            .Should().HaveNameEndingWith("CommandHandler")
            .Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_ShouldEndWith_QueryHandler()
    {
        Classes().That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And().ResideInNamespaceMatching(@".*\.Queries(\..*)?")
            .Should().HaveNameEndingWith("QueryHandler")
            .Check(Architecture);
    }

    [Fact]
    public void Commands_ShouldEndWith_Command()
    {
        Types().That()
            .ImplementInterface(typeof(MediatR.IRequest<>))
            .And().ResideInNamespaceMatching(@".*\.Commands(\..*)?")
            .Should().HaveNameEndingWith("Command")
            .Check(Architecture);
    }

    [Fact]
    public void Queries_ShouldEndWith_Query()
    {
        Types().That()
            .ImplementInterface(typeof(MediatR.IRequest<>))
            .And().ResideInNamespaceMatching(@".*\.Queries(\..*)?")
            .Should().HaveNameEndingWith("Query")
            .Check(Architecture);
    }

    [Fact]
    public void Validators_ShouldEndWith_Validator()
    {
        Classes().That()
            .AreAssignableTo(typeof(FluentValidation.IValidator))
            .Should().HaveNameEndingWith("Validator")
            .Check(Architecture);
    }
}
