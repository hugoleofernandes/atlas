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
                name: "idempotency_entries",
                schema: "atlas_staff",
                columns: table => new
                {
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_entries", x => new { x.IdempotencyKey, x.HandlerName });
                });

            migrationBuilder.CreateTable(
                name: "outboxes",
                schema: "atlas_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentOutboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TraceParent = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: true),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    LockId = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeadLetteredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outboxes_outboxes_ParentOutboxMessageId",
                        column: x => x.ParentOutboxMessageId,
                        principalSchema: "atlas_staff",
                        principalTable: "outboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.CreateTable(
                name: "outbox_handler_executions",
                schema: "atlas_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandlerName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_handler_executions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outbox_handler_executions_outboxes_OutboxMessageId",
                        column: x => x.OutboxMessageId,
                        principalSchema: "atlas_staff",
                        principalTable: "outboxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_outbox_handler_executions_AttemptedAt",
                schema: "atlas_staff",
                table: "outbox_handler_executions",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_handler_executions_HandlerName_Status",
                schema: "atlas_staff",
                table: "outbox_handler_executions",
                columns: new[] { "HandlerName", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_handler_executions_OutboxMessageId",
                schema: "atlas_staff",
                table: "outbox_handler_executions",
                column: "OutboxMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_IdempotencyKey",
                schema: "atlas_staff",
                table: "outboxes",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_Module",
                schema: "atlas_staff",
                table: "outboxes",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_ParentOutboxMessageId",
                schema: "atlas_staff",
                table: "outboxes",
                column: "ParentOutboxMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_outboxes_ProcessedOn_DeadLetteredOn_FailedAt_LockedUntil_Oc~",
                schema: "atlas_staff",
                table: "outboxes",
                columns: new[] { "ProcessedOn", "DeadLetteredOn", "FailedAt", "LockedUntil", "OccurredOn" });

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
                name: "idempotency_entries",
                schema: "atlas_staff");

            migrationBuilder.DropTable(
                name: "outbox_handler_executions",
                schema: "atlas_staff");

            migrationBuilder.DropTable(
                name: "staff_members",
                schema: "atlas_staff");

            migrationBuilder.DropTable(
                name: "outboxes",
                schema: "atlas_staff");
        }
    }
}
