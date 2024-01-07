using Microsoft.EntityFrameworkCore.Migrations;

namespace Centurion.WebhookSender.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "webhook_settings",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    success_url = table.Column<string>(type: "text", nullable: true),
                    failure_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_settings", x => x.user_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "webhook_settings");
        }
    }
}
