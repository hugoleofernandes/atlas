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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
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
                schema: "atlas_staff",
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
                name: "outbox_messages",
                schema: "atlas_staff",
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
                    origin = table.Column<string>(type: "text", nullable: false, defaultValue: "Automatic"),
                    resubmitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resubmitted_by_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    lock_id = table.Column<Guid>(type: "uuid", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dead_lettered_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_outbox_messages_outbox_messages_parent_outbox_message_id",
                        column: x => x.parent_outbox_message_id,
                        principalSchema: "atlas_staff",
                        principalTable: "outbox_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "staff_members",
                schema: "atlas_staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_staff_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_handler_executions",
                schema: "atlas_staff",
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
                        name: "fk_outbox_handler_executions_outbox_messages_outbox_message_id",
                        column: x => x.outbox_message_id,
                        principalSchema: "atlas_staff",
                        principalTable: "outbox_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audits_entity_type_id",
                schema: "atlas_staff",
                table: "audits",
                column: "entity_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_audits_occurred_at_utc",
                schema: "atlas_staff",
                table: "audits",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id",
                schema: "atlas_staff",
                table: "audits",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id_entity_type_id_action_occurred_at_utc",
                schema: "atlas_staff",
                table: "audits",
                columns: new[] { "tenant_id", "entity_type_id", "action", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id_entity_type_id_entity_id",
                schema: "atlas_staff",
                table: "audits",
                columns: new[] { "tenant_id", "entity_type_id", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id_entity_type_id_occurred_at_utc",
                schema: "atlas_staff",
                table: "audits",
                columns: new[] { "tenant_id", "entity_type_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_attempted_at",
                schema: "atlas_staff",
                table: "outbox_handler_executions",
                column: "attempted_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_handler_name_status",
                schema: "atlas_staff",
                table: "outbox_handler_executions",
                columns: new[] { "handler_name", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_outbox_message_id",
                schema: "atlas_staff",
                table: "outbox_handler_executions",
                column: "outbox_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_idempotency_key",
                schema: "atlas_staff",
                table: "outbox_messages",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_module",
                schema: "atlas_staff",
                table: "outbox_messages",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_parent_outbox_message_id",
                schema: "atlas_staff",
                table: "outbox_messages",
                column: "parent_outbox_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_dead_lettered_on_failed_at_loc",
                schema: "atlas_staff",
                table: "outbox_messages",
                columns: new[] { "processed_on", "dead_lettered_on", "failed_at", "locked_until", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_id",
                schema: "atlas_staff",
                table: "outbox_messages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_type",
                schema: "atlas_staff",
                table: "outbox_messages",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_staff_members_tenant_id_user_id",
                schema: "atlas_staff",
                table: "staff_members",
                columns: new[] { "tenant_id", "user_id" },
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
                name: "outbox_messages",
                schema: "atlas_staff");
        }
    }
}
