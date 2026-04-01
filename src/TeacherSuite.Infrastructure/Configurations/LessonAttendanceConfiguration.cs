using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class LessonAttendanceConfiguration : IEntityTypeConfiguration<LessonAttendance>
{
    public void Configure(EntityTypeBuilder<LessonAttendance> builder)
    {
        builder.HasKey(la => la.Id);

        builder.HasIndex(la => new { la.LessonId, la.GroupId })
               .IsUnique();

        builder.HasOne(la => la.Lesson)
               .WithMany(l => l.Attendances)
               .HasForeignKey(la => la.LessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(la => la.Group)
               .WithMany()
               .HasForeignKey(la => la.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
