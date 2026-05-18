using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Staff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Staff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atlas_staff");

            migrationBuilder.CreateTable(
                name: "audits",
                schema: "atlas_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangesJson = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outboxes",
                schema: "atlas_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    LockId = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeadLetteredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staff_members",
                schema: "atlas_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_members", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audits_EntityName",
                schema: "atlas_staff",
                table: "audits",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_audits_OccurredAtUtc",
                schema: "atlas_staff",
                table: "audits",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_audits_TenantId",
                schema: "atlas_staff",
                table: "audits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_Module",
                schema: "atlas_staff",
                table: "outboxes",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_ProcessedOn_DeadLetteredOn_LockedUntil_OccurredOn",
                schema: "atlas_staff",
                table: "outboxes",
                columns: new[] { "ProcessedOn", "DeadLetteredOn", "LockedUntil", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_TenantId",
                schema: "atlas_staff",
                table: "outboxes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_Type",
                schema: "atlas_staff",
                table: "outboxes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_staff_members_TenantId_UserId",
                schema: "atlas_staff",
                table: "staff_members",
                columns: new[] { "TenantId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audits",
                schema: "atlas_staff");

            migrationBuilder.DropTable(
                name: "outboxes",
                schema: "atlas_staff");

            migrationBuilder.DropTable(
                name: "staff_members",
                schema: "atlas_staff");
        }
    }
}
