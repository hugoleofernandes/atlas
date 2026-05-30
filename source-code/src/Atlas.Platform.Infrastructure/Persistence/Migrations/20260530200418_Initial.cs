using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atlas_platform");

            migrationBuilder.CreateTable(
                name: "audits",
                schema: "atlas_platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changes_json = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_entries",
                schema: "atlas_platform",
                columns: table => new
                {
                    idempotency_key = table.Column<Guid>(type: "uuid", nullable: false),
                    handler_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotency_entries", x => new { x.idempotency_key, x.handler_name });
                });

            migrationBuilder.CreateTable(
                name: "modules",
                schema: "atlas_platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outboxes",
                schema: "atlas_platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_outbox_message_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attempt_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trace_parent = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: true),
                    module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    lock_id = table.Column<Guid>(type: "uuid", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dead_lettered_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outboxes", x => x.id);
                    table.ForeignKey(
                        name: "fk_outboxes_outboxes_parent_outbox_message_id",
                        column: x => x.parent_outbox_message_id,
                        principalSchema: "atlas_platform",
                        principalTable: "outboxes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "systems",
                schema: "atlas_platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_systems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entity_types",
                schema: "atlas_platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    schema = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entity_types", x => x.id);
                    table.ForeignKey(
                        name: "fk_entity_types_modules_module_id",
                        column: x => x.module_id,
                        principalSchema: "atlas_platform",
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outbox_handler_executions",
                schema: "atlas_platform",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    outbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handler_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_handler_executions", x => x.id);
                    table.ForeignKey(
                        name: "fk_outbox_handler_executions_outbox_message_outbox_message_id",
                        column: x => x.outbox_message_id,
                        principalSchema: "atlas_platform",
                        principalTable: "outboxes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audits_entity_name",
                schema: "atlas_platform",
                table: "audits",
                column: "entity_name");

            migrationBuilder.CreateIndex(
                name: "ix_audits_occurred_at_utc",
                schema: "atlas_platform",
                table: "audits",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id",
                schema: "atlas_platform",
                table: "audits",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_entity_types_module_id_name",
                schema: "atlas_platform",
                table: "entity_types",
                columns: new[] { "module_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_modules_name",
                schema: "atlas_platform",
                table: "modules",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_attempted_at",
                schema: "atlas_platform",
                table: "outbox_handler_executions",
                column: "attempted_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_handler_name_status",
                schema: "atlas_platform",
                table: "outbox_handler_executions",
                columns: new[] { "handler_name", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_outbox_message_id",
                schema: "atlas_platform",
                table: "outbox_handler_executions",
                column: "outbox_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_outboxes_idempotency_key",
                schema: "atlas_platform",
                table: "outboxes",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_outboxes_module",
                schema: "atlas_platform",
                table: "outboxes",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "ix_outboxes_parent_outbox_message_id",
                schema: "atlas_platform",
                table: "outboxes",
                column: "parent_outbox_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_outboxes_processed_on_dead_lettered_on_failed_at_locked_unt",
                schema: "atlas_platform",
                table: "outboxes",
                columns: new[] { "processed_on", "dead_lettered_on", "failed_at", "locked_until", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "ix_outboxes_tenant_id",
                schema: "atlas_platform",
                table: "outboxes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_outboxes_type",
                schema: "atlas_platform",
                table: "outboxes",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_systems_name",
                schema: "atlas_platform",
                table: "systems",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audits",
                schema: "atlas_platform");

            migrationBuilder.DropTable(
                name: "entity_types",
                schema: "atlas_platform");

            migrationBuilder.DropTable(
                name: "idempotency_entries",
                schema: "atlas_platform");

            migrationBuilder.DropTable(
                name: "outbox_handler_executions",
                schema: "atlas_platform");

            migrationBuilder.DropTable(
                name: "systems",
                schema: "atlas_platform");

            migrationBuilder.DropTable(
                name: "modules",
                schema: "atlas_platform");

            migrationBuilder.DropTable(
                name: "outboxes",
                schema: "atlas_platform");
        }
    }
}
