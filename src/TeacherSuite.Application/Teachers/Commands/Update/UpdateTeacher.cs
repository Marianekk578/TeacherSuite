using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Teachers.Commands.Update;

public record UpdateTeacherCommand(Guid Id, string? FirstName, string? LastName, string Email, string PhoneNumber, DateTimeOffset DateOfBirth) : IRequest<Unit>, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.AllTeachers];
}

public class UpdateTeacherHandler(IApplicationDbContext context) : IRequestHandler<UpdateTeacherCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Teachers.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.Email = request.Email;
        entity.PhoneNumber = request.PhoneNumber;
        entity.DateOfBirth = request.DateOfBirth;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}