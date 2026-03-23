using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<GroupCourse> GroupCourses => Set<GroupCourse>();
        public DbSet<AgeGroup> AgeGroups => Set<AgeGroup>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
        public DbSet<ProgrammingLanguage> ProgrammingLanguages => Set<ProgrammingLanguage>();
        public DbSet<TeacherProgrammingLanguage> TeacherProgrammingLanguages => Set<TeacherProgrammingLanguage>();
        public DbSet<CourseProgrammingLanguage> CourseProgrammingLanguages => Set<CourseProgrammingLanguage>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<LessonAttendance> LessonAttendances => Set<LessonAttendance>();
        public DbSet<LessonSuggestion> LessonSuggestions => Set<LessonSuggestion>();
        public DbSet<SuggestionVote> SuggestionVotes => Set<SuggestionVote>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}