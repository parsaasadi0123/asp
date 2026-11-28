using InstaPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InstaPilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(InstaPilotDbContext))]
[Migration("202611260001_InitialIdentityWorkspace")]
public sealed class InitialIdentityWorkspace : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE [AspNetRoles] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AspNetRoles] PRIMARY KEY,
                [Name] nvarchar(256) NULL,
                [NormalizedName] nvarchar(256) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL
            );
            CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

            CREATE TABLE [AspNetUsers] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AspNetUsers] PRIMARY KEY,
                [UserName] nvarchar(256) NULL,
                [NormalizedUserName] nvarchar(256) NULL,
                [Email] nvarchar(256) NULL,
                [NormalizedEmail] nvarchar(256) NULL,
                [EmailConfirmed] bit NOT NULL,
                [PasswordHash] nvarchar(max) NULL,
                [SecurityStamp] nvarchar(max) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL,
                [PhoneNumber] nvarchar(max) NULL,
                [PhoneNumberConfirmed] bit NOT NULL,
                [TwoFactorEnabled] bit NOT NULL,
                [LockoutEnd] datetimeoffset NULL,
                [LockoutEnabled] bit NOT NULL,
                [AccessFailedCount] int NOT NULL,
                [DisplayName] nvarchar(max) NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [IsActive] bit NOT NULL
            );
            CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
            CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

            CREATE TABLE [Workspaces] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_Workspaces] PRIMARY KEY,
                [Name] nvarchar(120) NOT NULL,
                [Slug] nvarchar(80) NOT NULL,
                [CreatedByUserId] uniqueidentifier NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [UpdatedAtUtc] datetime2 NULL,
                [IsDeleted] bit NOT NULL
            );
            CREATE UNIQUE INDEX [IX_Workspaces_Slug] ON [Workspaces] ([Slug]);

            CREATE TABLE [AspNetRoleClaims] (
                [Id] int IDENTITY NOT NULL CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY,
                [RoleId] uniqueidentifier NOT NULL,
                [ClaimType] nvarchar(max) NULL,
                [ClaimValue] nvarchar(max) NULL,
                CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

            CREATE TABLE [AspNetUserClaims] (
                [Id] int IDENTITY NOT NULL CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY,
                [UserId] uniqueidentifier NOT NULL,
                [ClaimType] nvarchar(max) NULL,
                [ClaimValue] nvarchar(max) NULL,
                CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

            CREATE TABLE [AspNetUserLogins] (
                [LoginProvider] nvarchar(128) NOT NULL,
                [ProviderKey] nvarchar(128) NOT NULL,
                [ProviderDisplayName] nvarchar(max) NULL,
                [UserId] uniqueidentifier NOT NULL,
                CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
                CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

            CREATE TABLE [AspNetUserRoles] (
                [UserId] uniqueidentifier NOT NULL,
                [RoleId] uniqueidentifier NOT NULL,
                CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
                CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

            CREATE TABLE [AspNetUserTokens] (
                [UserId] uniqueidentifier NOT NULL,
                [LoginProvider] nvarchar(128) NOT NULL,
                [Name] nvarchar(128) NOT NULL,
                [Value] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
                CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );

            CREATE TABLE [WorkspaceMemberships] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_WorkspaceMemberships] PRIMARY KEY,
                [WorkspaceId] uniqueidentifier NOT NULL,
                [UserId] uniqueidentifier NOT NULL,
                [Role] int NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [UpdatedAtUtc] datetime2 NULL,
                [IsDeleted] bit NOT NULL,
                CONSTRAINT [FK_WorkspaceMemberships_Workspaces_WorkspaceId] FOREIGN KEY ([WorkspaceId]) REFERENCES [Workspaces] ([Id]) ON DELETE NO ACTION,
                CONSTRAINT [FK_WorkspaceMemberships_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX [IX_WorkspaceMemberships_WorkspaceId_UserId] ON [WorkspaceMemberships] ([WorkspaceId], [UserId]);
            CREATE INDEX [IX_WorkspaceMemberships_UserId] ON [WorkspaceMemberships] ([UserId]);

            CREATE TABLE [RefreshTokens] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_RefreshTokens] PRIMARY KEY,
                [UserId] uniqueidentifier NOT NULL,
                [FamilyId] uniqueidentifier NOT NULL,
                [TokenHash] nvarchar(64) NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                [ExpiresAtUtc] datetime2 NOT NULL,
                [RevokedAtUtc] datetime2 NULL,
                [ReplacedByTokenId] uniqueidentifier NULL,
                [CreatedByIp] nvarchar(64) NULL,
                [RowVersion] rowversion NOT NULL,
                CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
            CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAtUtc] ON [RefreshTokens] ([UserId], [ExpiresAtUtc]);
            CREATE INDEX [IX_RefreshTokens_FamilyId_RevokedAtUtc] ON [RefreshTokens] ([FamilyId], [RevokedAtUtc]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS [RefreshTokens];
            DROP TABLE IF EXISTS [WorkspaceMemberships];
            DROP TABLE IF EXISTS [AspNetUserTokens];
            DROP TABLE IF EXISTS [AspNetUserRoles];
            DROP TABLE IF EXISTS [AspNetUserLogins];
            DROP TABLE IF EXISTS [AspNetUserClaims];
            DROP TABLE IF EXISTS [AspNetRoleClaims];
            DROP TABLE IF EXISTS [Workspaces];
            DROP TABLE IF EXISTS [AspNetUsers];
            DROP TABLE IF EXISTS [AspNetRoles];
            """);
    }
}
