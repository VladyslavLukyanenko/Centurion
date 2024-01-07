using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Centurion.CloudManager.Infra.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateSequence(
                name: "node_snapshot_hi_lo_sequence",
                schema: "public",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "image_info",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    required_spawn_parameters = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_image_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "node_snapshot",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    node_id = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    provider_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_node_snapshot", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_node_snapshot_user_id",
                schema: "public",
                table: "node_snapshot",
                column: "user_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "image_info",
                schema: "public");

            migrationBuilder.DropTable(
                name: "node_snapshot",
                schema: "public");

            migrationBuilder.DropSequence(
                name: "node_snapshot_hi_lo_sequence",
                schema: "public");
        }
    }
}
