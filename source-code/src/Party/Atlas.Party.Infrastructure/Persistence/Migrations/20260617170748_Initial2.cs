using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Party.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                schema: "atlas_party",
                table: "party_contacts");

            migrationBuilder.DropColumn(
                name: "phone",
                schema: "atlas_party",
                table: "party_contacts");

            migrationBuilder.AddColumn<string>(
                name: "value",
                schema: "atlas_party",
                table: "party_contacts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "value",
                schema: "atlas_party",
                table: "party_contacts");

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "atlas_party",
                table: "party_contacts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                schema: "atlas_party",
                table: "party_contacts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
