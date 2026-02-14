using Bogus;
using TeacherSuite.Application.Teachers.Commands.Create;

namespace TeacherSuite.Application.Teachers.Commands.SeedTestTeachers;

public record SeedTestTeachersCommand(int Count = 1000) : IRequest<int>;

public class SeedTestTeachersHandler(ISender sender) : IRequestHandler<SeedTestTeachersCommand, int>
{
    public async Task<int> Handle(SeedTestTeachersCommand request, CancellationToken cancellationToken)
    {
        var faker = new Faker<CreateTeacherCommand>()
            .CustomInstantiator(f =>
            {
                var firstName = f.Name.FirstName();
                return new CreateTeacherCommand(
                    firstName,
                    "Testowski",
                    $"{firstName.ToLower()}.testowski{f.UniqueIndex}@testoowski.pl",
                    f.Phone.PhoneNumber("+48 ### ### ###"),
                    new DateTimeOffset(f.Date.Between(
                        DateTime.Now.AddYears(-65),
                        DateTime.Now.AddYears(-18)), TimeSpan.Zero)
                );
            });

        var commands = faker.Generate(request.Count);
        var created = 0;

        foreach (var command in commands)
        {
            await sender.Send(command, cancellationToken);
            created++;
        }

        return created;
    }
}
