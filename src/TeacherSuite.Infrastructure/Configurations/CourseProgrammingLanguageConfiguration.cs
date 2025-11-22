using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class CourseProgrammingLanguageConfiguration : IEntityTypeConfiguration<CourseProgrammingLanguage>
{
    public void Configure(EntityTypeBuilder<CourseProgrammingLanguage> builder)
    {
        builder.HasKey(cpl => new { cpl.CourseId, cpl.ProgrammingLanguageId });

        builder.HasOne(cpl => cpl.Course)
               .WithMany(c => c.CourseProgrammingLanguages)
               .HasForeignKey(cpl => cpl.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cpl => cpl.ProgrammingLanguage)
               .WithMany(pl => pl.CourseProgrammingLanguages)
               .HasForeignKey(cpl => cpl.ProgrammingLanguageId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}