using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Staff.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial2_Staff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "origin",
                schema: "atlas_staff",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "Automatic");

            migrationBuilder.AddColumn<string>(
                name: "resubmitted_by_email",
                schema: "atlas_staff",
                table: "outbox_messages",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "resubmitted_by_user_id",
                schema: "atlas_staff",
                table: "outbox_messages",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "origin",
                schema: "atlas_staff",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "resubmitted_by_email",
                schema: "atlas_staff",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "resubmitted_by_user_id",
                schema: "atlas_staff",
                table: "outbox_messages");
        }
    }
}
