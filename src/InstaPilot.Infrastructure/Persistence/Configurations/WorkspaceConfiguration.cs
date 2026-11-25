using InstaPilot.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace InstaPilot.Infrastructure.Persistence.Configurations;
public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
