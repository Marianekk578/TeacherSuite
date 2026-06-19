using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class LessonSuggestionConfiguration : IEntityTypeConfiguration<LessonSuggestion>
{
    public void Configure(EntityTypeBuilder<LessonSuggestion> builder)
    {
        builder.HasKey(ls => ls.Id);

        builder.HasOne(ls => ls.Lesson)
               .WithMany(l => l.Suggestions)
               .HasForeignKey(ls => ls.LessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ls => ls.Teacher)
               .WithMany()
               .HasForeignKey(ls => ls.TeacherId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ls => ls.Content)
               .IsRequired()
               .HasMaxLength(4000);

        builder.Property(ls => ls.SelectedText)
               .HasMaxLength(1000);
    }
}
