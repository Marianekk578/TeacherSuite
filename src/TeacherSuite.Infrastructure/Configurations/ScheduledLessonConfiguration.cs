using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class ScheduledLessonConfiguration : IEntityTypeConfiguration<ScheduledLesson>
{
    public void Configure(EntityTypeBuilder<ScheduledLesson> builder)
    {
        builder.HasKey(sl => sl.Id);

        builder.HasIndex(sl => new { sl.LessonId, sl.GroupId, sl.ScheduledStart })
               .IsUnique();

        builder.HasOne(sl => sl.Lesson)
               .WithMany()
               .HasForeignKey(sl => sl.LessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.Group)
               .WithMany()
               .HasForeignKey(sl => sl.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentLessonAttendanceConfiguration : IEntityTypeConfiguration<StudentLessonAttendance>
{
    public void Configure(EntityTypeBuilder<StudentLessonAttendance> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.HasIndex(sa => new { sa.ScheduledLessonId, sa.StudentId })
               .IsUnique();

        builder.HasOne(sa => sa.ScheduledLesson)
               .WithMany(sl => sl.StudentAttendances)
               .HasForeignKey(sa => sa.ScheduledLessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Student)
               .WithMany()
               .HasForeignKey(sa => sa.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
