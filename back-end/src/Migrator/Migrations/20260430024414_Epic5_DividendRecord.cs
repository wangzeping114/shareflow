using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Epic5_DividendRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dividend_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_revenue_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revenue_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    share_permille = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    dividend_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dividend_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_dividend_records_platform_revenues_platform_revenue_id",
                        column: x => x.platform_revenue_id,
                        principalTable: "platform_revenues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dividend_records_project_slots_slot_id",
                        column: x => x.slot_id,
                        principalTable: "project_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dividend_records_users_investor_user_id",
                        column: x => x.investor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dividend_records_video_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "video_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dividend_records_investor_user_id",
                table: "dividend_records",
                column: "investor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_dividend_records_project_id",
                table: "dividend_records",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_dividend_records_revenue_id",
                table: "dividend_records",
                column: "platform_revenue_id");

            migrationBuilder.CreateIndex(
                name: "IX_dividend_records_slot_id",
                table: "dividend_records",
                column: "slot_id");

            migrationBuilder.CreateIndex(
                name: "ix_dividend_records_status",
                table: "dividend_records",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dividend_records");
        }
    }
}
