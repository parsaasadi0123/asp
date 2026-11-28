using InstaPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InstaPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InstaPilotDbContext))]
[Migration("202611280001_MetaOAuthAccounts")]
public sealed class MetaOAuthAccounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE [MetaAccounts] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MetaAccounts] PRIMARY KEY,
                [WorkspaceId] uniqueidentifier NOT NULL,
                [MetaUserId] nvarchar(100) NOT NULL,
                [DisplayName] nvarchar(200) NOT NULL,
                [ProtectedAccessToken] nvarchar(4000) NOT NULL,
                [AccessTokenExpiresAtUtc] datetime2 NULL,
                [ConnectedByUserId] uniqueidentifier NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [UpdatedAtUtc] datetime2 NULL,
                [IsDeleted] bit NOT NULL,
                CONSTRAINT [FK_MetaAccounts_Workspaces_WorkspaceId] FOREIGN KEY ([WorkspaceId]) REFERENCES [Workspaces] ([Id]) ON DELETE NO ACTION
            );
            CREATE UNIQUE INDEX [IX_MetaAccounts_WorkspaceId_MetaUserId] ON [MetaAccounts] ([WorkspaceId], [MetaUserId]);

            CREATE TABLE [MetaOAuthStates] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_MetaOAuthStates] PRIMARY KEY,
                [WorkspaceId] uniqueidentifier NOT NULL,
                [UserId] uniqueidentifier NOT NULL,
                [ExpiresAtUtc] datetime2 NOT NULL,
                [ConsumedAtUtc] datetime2 NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [UpdatedAtUtc] datetime2 NULL,
                [IsDeleted] bit NOT NULL,
                CONSTRAINT [FK_MetaOAuthStates_Workspaces_WorkspaceId] FOREIGN KEY ([WorkspaceId]) REFERENCES [Workspaces] ([Id]) ON DELETE NO ACTION,
                CONSTRAINT [FK_MetaOAuthStates_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_MetaOAuthStates_ExpiresAtUtc] ON [MetaOAuthStates] ([ExpiresAtUtc]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS [MetaOAuthStates];
            DROP TABLE IF EXISTS [MetaAccounts];
            """);
    }
}
