using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class SuggestionVoteConfiguration : IEntityTypeConfiguration<SuggestionVote>
{
    public void Configure(EntityTypeBuilder<SuggestionVote> builder)
    {
        builder.HasKey(sv => new { sv.SuggestionId, sv.TeacherId });

        builder.HasOne(sv => sv.Suggestion)
               .WithMany(s => s.Votes)
               .HasForeignKey(sv => sv.SuggestionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sv => sv.Teacher)
               .WithMany()
               .HasForeignKey(sv => sv.TeacherId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
