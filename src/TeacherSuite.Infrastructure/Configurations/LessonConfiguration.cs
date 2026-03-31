using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasIndex(l => new { l.CourseId, l.Order })
               .IsUnique();

        builder.HasOne(l => l.Course)
               .WithMany(c => c.Lessons)
               .HasForeignKey(l => l.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(l => l.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(l => l.Description)
               .HasMaxLength(2000);

        builder.Property(l => l.AlbumId)
               .HasMaxLength(500);
    }
}

public class RequirementIconConfiguration : IEntityTypeConfiguration<RequirementIcon>
{
    public void Configure(EntityTypeBuilder<RequirementIcon> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Emoji)
               .IsRequired()
               .HasMaxLength(10);

        builder.Property(r => r.Label)
               .IsRequired()
               .HasMaxLength(200);
    }
}

public class LessonRequirementIconConfiguration : IEntityTypeConfiguration<LessonRequirementIcon>
{
    public void Configure(EntityTypeBuilder<LessonRequirementIcon> builder)
    {
        builder.HasKey(lr => new { lr.LessonId, lr.RequirementIconId });

        builder.HasOne(lr => lr.Lesson)
               .WithMany(l => l.LessonRequirementIcons)
               .HasForeignKey(lr => lr.LessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lr => lr.RequirementIcon)
               .WithMany(r => r.LessonRequirementIcons)
               .HasForeignKey(lr => lr.RequirementIconId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
