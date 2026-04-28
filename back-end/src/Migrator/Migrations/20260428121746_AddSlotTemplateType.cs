using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotTemplateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "template_type",
                table: "project_slots",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "OverseasEnglish");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "template_type",
                table: "project_slots");
        }
    }
}
