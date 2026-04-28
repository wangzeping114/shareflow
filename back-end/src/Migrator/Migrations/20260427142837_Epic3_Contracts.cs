using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Epic3_Contracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    template_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sign_token = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    sign_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    signature_data_url = table.Column<string>(type: "text", nullable: true),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pdf_storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    contract_snapshot = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.id);
                    table.ForeignKey(
                        name: "FK_contracts_project_slots_slot_id",
                        column: x => x.slot_id,
                        principalTable: "project_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_users_investor_user_id",
                        column: x => x.investor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_video_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "video_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contracts_investor_user_id",
                table: "contracts",
                column: "investor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_project_id",
                table: "contracts",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_sign_token",
                table: "contracts",
                column: "sign_token",
                unique: true,
                filter: "sign_token IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_slot_id",
                table: "contracts",
                column: "slot_id");

            migrationBuilder.CreateIndex(
                name: "ix_contracts_status",
                table: "contracts",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contracts");
        }
    }
}
