using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddClientUserIdToLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "client_user_id",
                table: "leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_leads_client_user_id",
                table: "leads",
                column: "client_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_leads_users_client_user_id",
                table: "leads",
                column: "client_user_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leads_users_client_user_id",
                table: "leads");

            migrationBuilder.DropIndex(
                name: "ix_leads_client_user_id",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "client_user_id",
                table: "leads");
        }
    }
}
