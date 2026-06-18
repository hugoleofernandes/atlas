using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Party.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atlas_party");

            migrationBuilder.CreateTable(
                name: "audits",
                schema: "atlas_party",
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
                schema: "atlas_party",
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
                schema: "atlas_party",
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
                        principalSchema: "atlas_party",
                        principalTable: "outbox_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "parties",
                schema: "atlas_party",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tax_number = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    party_type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    trade_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    legal_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_handler_executions",
                schema: "atlas_party",
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
                        principalSchema: "atlas_party",
                        principalTable: "outbox_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_addresses",
                schema: "atlas_party",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_party_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_party_addresses_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "atlas_party",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_classifications",
                schema: "atlas_party",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    since = table.Column<DateOnly>(type: "date", nullable: false),
                    until = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_party_classifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_party_classifications_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "atlas_party",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "party_contacts",
                schema: "atlas_party",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_party_contacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_party_contacts_parties_party_id",
                        column: x => x.party_id,
                        principalSchema: "atlas_party",
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audits_entity_type_id",
                schema: "atlas_party",
                table: "audits",
                column: "entity_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_audits_occurred_at_utc",
                schema: "atlas_party",
                table: "audits",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id",
                schema: "atlas_party",
                table: "audits",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id_entity_type_id_action_occurred_at_utc",
                schema: "atlas_party",
                table: "audits",
                columns: new[] { "tenant_id", "entity_type_id", "action", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id_entity_type_id_entity_id",
                schema: "atlas_party",
                table: "audits",
                columns: new[] { "tenant_id", "entity_type_id", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audits_tenant_id_entity_type_id_occurred_at_utc",
                schema: "atlas_party",
                table: "audits",
                columns: new[] { "tenant_id", "entity_type_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_attempted_at",
                schema: "atlas_party",
                table: "outbox_handler_executions",
                column: "attempted_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_handler_name_status",
                schema: "atlas_party",
                table: "outbox_handler_executions",
                columns: new[] { "handler_name", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_handler_executions_outbox_message_id",
                schema: "atlas_party",
                table: "outbox_handler_executions",
                column: "outbox_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_idempotency_key",
                schema: "atlas_party",
                table: "outbox_messages",
                column: "idempotency_key");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_module",
                schema: "atlas_party",
                table: "outbox_messages",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_parent_outbox_message_id",
                schema: "atlas_party",
                table: "outbox_messages",
                column: "parent_outbox_message_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_dead_lettered_on_failed_at_loc",
                schema: "atlas_party",
                table: "outbox_messages",
                columns: new[] { "processed_on", "dead_lettered_on", "failed_at", "locked_until", "occurred_on" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_id",
                schema: "atlas_party",
                table: "outbox_messages",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_type",
                schema: "atlas_party",
                table: "outbox_messages",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_parties_tenant_id_tax_number",
                schema: "atlas_party",
                table: "parties",
                columns: new[] { "tenant_id", "tax_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_party_addresses_party_id",
                schema: "atlas_party",
                table: "party_addresses",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "ix_party_classifications_party_id_type",
                schema: "atlas_party",
                table: "party_classifications",
                columns: new[] { "party_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_party_contacts_party_id",
                schema: "atlas_party",
                table: "party_contacts",
                column: "party_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audits",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "idempotency_entries",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "outbox_handler_executions",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "party_addresses",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "party_classifications",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "party_contacts",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "atlas_party");

            migrationBuilder.DropTable(
                name: "parties",
                schema: "atlas_party");
        }
    }
}
