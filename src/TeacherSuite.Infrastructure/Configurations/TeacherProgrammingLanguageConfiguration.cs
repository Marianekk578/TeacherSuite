using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class TeacherProgrammingLanguageConfiguration : IEntityTypeConfiguration<TeacherProgrammingLanguage>
{
    public void Configure(EntityTypeBuilder<TeacherProgrammingLanguage> builder)
    {
        builder.HasKey(tpl => new { tpl.TeacherId, tpl.ProgrammingLanguageId });

        builder.HasOne(tpl => tpl.Teacher)
               .WithMany(t => t.TeacherProgrammingLanguages)
               .HasForeignKey(tpl => tpl.TeacherId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tpl => tpl.ProgrammingLanguage)
               .WithMany(pl => pl.TeacherProgrammingLanguages)
               .HasForeignKey(tpl => tpl.ProgrammingLanguageId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}