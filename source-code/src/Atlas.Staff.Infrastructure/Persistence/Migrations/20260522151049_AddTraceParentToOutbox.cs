using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Staff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTraceParentToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TraceParent",
                schema: "atlas_staff",
                table: "outboxes",
                type: "character varying(55)",
                maxLength: 55,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TraceParent",
                schema: "atlas_staff",
                table: "outboxes");
        }
    }
}
