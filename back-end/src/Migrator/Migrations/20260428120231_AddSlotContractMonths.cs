using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotContractMonths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "contract_months",
                table: "project_slots",
                type: "integer",
                nullable: false,
                defaultValue: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_months",
                table: "project_slots");
        }
    }
}
