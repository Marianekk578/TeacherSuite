using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AgeGroup> AgeGroups { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Course> Courses { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupCourse> GroupCourses { get; }
    DbSet<Student> Students { get; }
    DbSet<StudentGroup> StudentGroups { get; }
    DbSet<ProgrammingLanguage> ProgrammingLanguages { get; }
    DbSet<TeacherProgrammingLanguage> TeacherProgrammingLanguages { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<LessonAttendance> LessonAttendances { get; }
    DbSet<LessonSuggestion> LessonSuggestions { get; }
    DbSet<SuggestionVote> SuggestionVotes { get; }
    DbSet<RequirementIcon> RequirementIcons { get; }
    DbSet<LessonRequirementIcon> LessonRequirementIcons { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
