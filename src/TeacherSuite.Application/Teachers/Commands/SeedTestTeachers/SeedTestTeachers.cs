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
                var birthDate = DateTime.SpecifyKind(
                    f.Date.Between(DateTime.Now.AddYears(-65), DateTime.Now.AddYears(-18)),
                    DateTimeKind.Utc);

                return new CreateTeacherCommand(
                    firstName,
                    "Testowski",
                    $"{firstName.ToLower()}.testowski{f.UniqueIndex}@testoowski.pl",
                    f.Phone.PhoneNumber("+48 ### ### ###"),
                    new DateTimeOffset(birthDate)
                );
            });

        var commands = faker.Generate(request.Count);

        foreach (var command in commands)
        {
            await sender.Send(command, cancellationToken);
        }

        return commands.Count;
    }
}
