using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class GroupCourseConfiguration : IEntityTypeConfiguration<GroupCourse>
{
    public void Configure(EntityTypeBuilder<GroupCourse> builder)
    {
        builder.HasKey(gc => gc.Id);

        builder.HasOne(gc => gc.Group)
               .WithMany(g => g.GroupCourses)
               .HasForeignKey(gc => gc.GroupId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gc => gc.Course)
               .WithMany(c => c.GroupCourses)
               .HasForeignKey(gc => gc.CourseId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}