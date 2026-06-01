using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddContractNoSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "contract_no_seq");

            migrationBuilder.AddColumn<int>(
                name: "contract_no",
                table: "contracts",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('contract_no_seq')");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_contract_no",
                table: "contracts",
                column: "contract_no",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_contracts_contract_no",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "contract_no",
                table: "contracts");

            migrationBuilder.DropSequence(
                name: "contract_no_seq");
        }
    }
}
