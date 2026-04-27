using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareFlow.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Epic2_VideoProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "video_projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    platform_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slot_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_slots = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    share_permille = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    client_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    VideoProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_slots_video_projects_VideoProjectId",
                        column: x => x.VideoProjectId,
                        principalTable: "video_projects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_slots_client_user_id",
                table: "project_slots",
                column: "client_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_slots_project_id",
                table: "project_slots",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_slots_VideoProjectId",
                table: "project_slots",
                column: "VideoProjectId");

            migrationBuilder.CreateIndex(
                name: "ix_video_projects_created_by",
                table: "video_projects",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_projects_status",
                table: "video_projects",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_slots");

            migrationBuilder.DropTable(
                name: "video_projects");
        }
    }
}
