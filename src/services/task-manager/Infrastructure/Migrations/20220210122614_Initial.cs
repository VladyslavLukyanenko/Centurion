using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Centurion.TaskManager.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.CreateTable(
                name: "checkout_task_group",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkout_task_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "preset",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_title = table.Column<string>(type: "text", nullable: false),
                    product_picture = table.Column<string>(type: "text", nullable: false),
                    expected_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    module = table.Column<string>(type: "text", nullable: false),
                    product_sku = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_preset", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                schema: "public",
                columns: table => new
                {
                    sku = table.Column<string>(type: "text", nullable: false),
                    module = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    link = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => new { x.module, x.sku });
                });

            migrationBuilder.CreateTable(
                name: "product_checked_out_event",
                schema: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    picture = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    formatted_price = table.Column<string>(type: "text", nullable: false),
                    thumbnail = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    mode = table.Column<string>(type: "text", nullable: false),
                    qty = table.Column<long>(type: "bigint", nullable: false),
                    delay = table.Column<Duration>(type: "interval", nullable: false),
                    profile = table.Column<string>(type: "text", nullable: false),
                    store = table.Column<string>(type: "text", nullable: false),
                    attrs = table.Column<string>(type: "jsonb", nullable: false),
                    processing_log = table.Column<string>(type: "jsonb", nullable: false),
                    task_id = table.Column<string>(type: "text", nullable: false),
                    shop_icon_url = table.Column<string>(type: "text", nullable: false),
                    shop_title = table.Column<string>(type: "text", nullable: false),
                    links = table.Column<string>(type: "jsonb", nullable: false),
                    sku = table.Column<string>(type: "text", nullable: false),
                    account = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<Duration>(type: "interval", nullable: false),
                    proxy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_checked_out_event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "checkout_task",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    profile_ids = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'"),
                    checkout_proxy_pool_id = table.Column<Guid>(type: "uuid", nullable: true),
                    monitor_proxy_pool_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    module = table.Column<string>(type: "text", nullable: false),
                    product_sku = table.Column<string>(type: "text", nullable: false),
                    config = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkout_task", x => x.id);
                    table.ForeignKey(
                        name: "fk_checkout_task_checkout_task_group_group_id",
                        column: x => x.group_id,
                        principalSchema: "public",
                        principalTable: "checkout_task_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_checkout_task_group_id",
                schema: "public",
                table: "checkout_task",
                column: "group_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checkout_task",
                schema: "public");

            migrationBuilder.DropTable(
                name: "preset",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_checked_out_event",
                schema: "events");

            migrationBuilder.DropTable(
                name: "checkout_task_group",
                schema: "public");
        }
    }
}
