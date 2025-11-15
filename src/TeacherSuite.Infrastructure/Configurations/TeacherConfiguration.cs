using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>

{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasMany(t => t.Groups)
               .WithOne(g => g.Teacher)
               .HasForeignKey(g => g.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

