using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddClientInitialPasswordToLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "client_initial_password",
                table: "leads",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_initial_password",
                table: "leads");
        }
    }
}
