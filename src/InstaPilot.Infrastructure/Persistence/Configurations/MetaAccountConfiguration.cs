using InstaPilot.Domain.Meta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstaPilot.Infrastructure.Persistence.Configurations;

public sealed class MetaAccountConfiguration : IEntityTypeConfiguration<MetaAccount>
{
    public void Configure(EntityTypeBuilder<MetaAccount> builder)
    {
        builder.ToTable("MetaAccounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MetaUserId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProtectedAccessToken).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => new { x.WorkspaceId, x.MetaUserId }).IsUnique();
        builder.HasOne<InstaPilot.Domain.Workspaces.Workspace>().WithMany()
            .HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
