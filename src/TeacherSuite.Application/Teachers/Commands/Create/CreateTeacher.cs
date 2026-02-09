using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Teachers.Commands.Create;

public record CreateTeacherCommand(string? FirstName, string? LastName, string Email, string PhoneNumber, DateTimeOffset DateOfBirth) : IRequest<Guid>;

public class CreateTeacherHandler : IRequestHandler<CreateTeacherCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateTeacherHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        var entity = new Teacher
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth
        };

        _context.Teachers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new TeacherCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}