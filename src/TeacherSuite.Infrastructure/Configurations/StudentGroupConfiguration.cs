using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class StudentGroupConfiguration : IEntityTypeConfiguration<StudentGroup>
{
    public void Configure(EntityTypeBuilder<StudentGroup> builder)
    {
        builder.HasKey(sg => sg.Id);

        builder.HasIndex(sg => new { sg.StudentId, sg.GroupId })
               .IsUnique();

        builder.HasOne(sg => sg.Student)
               .WithMany(s => s.StudentGroups)
               .HasForeignKey(sg => sg.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sg => sg.Group)
               .WithMany(g => g.StudentGroups)
               .HasForeignKey(sg => sg.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
