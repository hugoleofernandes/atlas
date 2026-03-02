using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atlas");

            migrationBuilder.CreateTable(
                name: "identity_users",
                schema: "atlas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identity_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "atlas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_memberships",
                schema: "atlas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "User"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_memberships_identity_users_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalSchema: "atlas",
                        principalTable: "identity_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_memberships_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "atlas",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_identity_users_ExternalId",
                schema: "atlas",
                table: "identity_users",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_memberships_IdentityUserId",
                schema: "atlas",
                table: "tenant_memberships",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_memberships_TenantId_Email",
                schema: "atlas",
                table: "tenant_memberships",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_memberships_TenantId_IdentityUserId",
                schema: "atlas",
                table: "tenant_memberships",
                columns: new[] { "TenantId", "IdentityUserId" },
                unique: true,
                filter: "\"IdentityUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                schema: "atlas",
                table: "tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_memberships",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "identity_users",
                schema: "atlas");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "atlas");
        }
    }
}
