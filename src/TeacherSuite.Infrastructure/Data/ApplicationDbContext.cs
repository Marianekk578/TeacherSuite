using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Interfaces;

namespace TeacherSuite.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<GroupCourse> GroupCourses => Set<GroupCourse>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<ProgrammingLanguage> ProgrammingLanguages => Set<ProgrammingLanguage>();
        public DbSet<TeacherProgrammingLanguage> TeacherProgrammingLanguages => Set<TeacherProgrammingLanguage>();
        public DbSet<CourseProgrammingLanguage> CourseProgrammingLanguages => Set<CourseProgrammingLanguage>();

        // IApplicationDbContext implementation
        IQueryable<AgeGroup> IApplicationDbContext.AgeGroups => Set<AgeGroup>();

        void IApplicationDbContext.Add<T>(T entity)
        {
            Set<T>().Add(entity);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}