using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Infrastructure.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);

        builder.HasMany(g => g.Students)
               .WithOne(s => s.Group)
               .HasForeignKey(s => s.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
