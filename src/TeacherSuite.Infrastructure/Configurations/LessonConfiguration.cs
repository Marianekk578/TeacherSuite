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

        builder.Property(l => l.MarkdownContent)
               .HasColumnType("text");

        builder.Property(l => l.MaterialFileName)
               .HasMaxLength(500);

        builder.Property(l => l.MaterialStorageKey)
               .HasMaxLength(500);

        builder.Property(l => l.AlbumId)
               .HasMaxLength(500);

        builder.Property(l => l.RequirementIcons)
               .HasColumnType("text");
    }
}
