using InstaPilot.Domain.Meta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InstaPilot.Infrastructure.Persistence.Configurations;

public sealed class MetaOAuthStateConfiguration : IEntityTypeConfiguration<MetaOAuthState>
{
    public void Configure(EntityTypeBuilder<MetaOAuthState> builder)
    {
        builder.ToTable("MetaOAuthStates");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasOne<InstaPilot.Domain.Workspaces.Workspace>().WithMany()
            .HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<InstaPilot.Infrastructure.Identity.ApplicationUser>().WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
