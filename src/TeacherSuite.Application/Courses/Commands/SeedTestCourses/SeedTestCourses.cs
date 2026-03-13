using Bogus;
using TeacherSuite.Application.Courses.Commands.Create;

namespace TeacherSuite.Application.Courses.Commands.SeedTestCourses;

public record SeedTestCoursesCommand(int Count = 100) : IRequest<int>;

public class SeedTestCoursesHandler(ISender sender) : IRequestHandler<SeedTestCoursesCommand, int>
{
    private static readonly int[] AgeGroupIds = [1, 2, 3, 4, 5];

    public async Task<int> Handle(SeedTestCoursesCommand request, CancellationToken cancellationToken)
    {
        var faker = new Faker<CreateCourseCommand>()
            .CustomInstantiator(f =>
            {
                var subject = f.PickRandom(CourseSubjects);
                var level = f.PickRandom("Beginner", "Intermediate", "Advanced");

                return new CreateCourseCommand(
                    $"Test - {subject} {level} {f.UniqueIndex}",
                    f.Lorem.Sentence(10, 5),
                    f.PickRandom(AgeGroupIds)
                );
            });

        var commands = faker.Generate(request.Count);

        foreach (var command in commands)
        {
            await sender.Send(command, cancellationToken);
        }

        return commands.Count;
    }

    private static readonly string[] CourseSubjects =
    [
        "Python Programming", "Java Fundamentals", "Web Development",
        "Data Structures", "Machine Learning", "Mobile App Development",
        "Game Development", "Robotics", "Cybersecurity", "Cloud Computing",
        "Database Design", "Algorithms", "C# Programming", "JavaScript",
        "React Development", "Angular Framework", "DevOps", "Linux Administration",
        "Networking", "AI Foundations"
    ];
}
