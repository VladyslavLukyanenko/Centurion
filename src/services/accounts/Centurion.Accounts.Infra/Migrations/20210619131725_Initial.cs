using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Centurion.Accounts.Infra.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "charge_backers");

            migrationBuilder.EnsureSchema(
                name: "products");

            migrationBuilder.EnsureSchema(
                name: "embeds");

            migrationBuilder.EnsureSchema(
                name: "forms");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.EnsureSchema(
                name: "orders");

            migrationBuilder.EnsureSchema(
                name: "web_hooks");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateSequence(
                name: "charge_backer_hi_lo_sequence",
                schema: "charge_backers",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "discord_embed_web_hook_binding_hi_lo_sequence",
                schema: "embeds",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "form_component_hi_lo_sequence",
                schema: "forms",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "form_hi_lo_sequence",
                schema: "forms",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "form_response_hi_lo_sequence",
                schema: "forms",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "license_key_hi_lo_sequence",
                schema: "products",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "member_role_hi_lo_sequence",
                schema: "security",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "plan_hi_lo_sequence",
                schema: "products",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "published_web_hook_hi_lo_sequence",
                schema: "web_hooks",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "release_hi_lo_sequence",
                schema: "products",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "role_hi_lo_sequence",
                schema: "identity",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "user_claim_hi_lo_sequence",
                schema: "identity",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "user_hi_lo_sequence",
                schema: "identity",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "user_member_role_binding_hi_lo_sequence",
                schema: "security",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "web_hook_binding_hi_lo_sequence",
                schema: "web_hooks",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "role",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    concurrency_stamp = table.Column<string>(type: "text", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    avatar = table.Column<string>(type: "text", nullable: true),
                    last_refreshed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    discord_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    stripe_customer_id = table.Column<string>(type: "text", nullable: true),
                    discord_roles = table.Column<string>(type: "jsonb", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: false),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "text", rowVersion: true, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "change_set",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_change_set", x => x.id);
                    table.ForeignKey(
                        name: "fk_change_set_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dashboard",
                schema: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_info_name = table.Column<string>(type: "text", nullable: false),
                    product_info_description = table.Column<string>(type: "text", nullable: false),
                    product_info_version = table.Column<string>(type: "text", nullable: false),
                    product_info_features = table.Column<string>(type: "jsonb", nullable: false),
                    product_info_logo_src = table.Column<string>(type: "text", nullable: true),
                    product_info_image_src = table.Column<string>(type: "text", nullable: true),
                    stripe_config_api_key = table.Column<string>(type: "text", nullable: true),
                    stripe_config_web_hook_endpoint_secret = table.Column<string>(type: "text", nullable: true),
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    discord_config_guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discord_config_role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    discord_config_access_token = table.Column<string>(type: "text", nullable: true),
                    discord_config_bot_access_token = table.Column<string>(type: "text", nullable: true),
                    discord_config_oauth_config_client_id = table.Column<string>(type: "text", nullable: false),
                    discord_config_oauth_config_client_secret = table.Column<string>(type: "text", nullable: false),
                    discord_config_oauth_config_redirect_url = table.Column<string>(type: "text", nullable: false),
                    discord_config_oauth_config_scope = table.Column<string>(type: "text", nullable: false),
                    time_zone_id = table.Column<string>(type: "text", nullable: false),
                    hosting_config_domain_name = table.Column<string>(type: "text", nullable: false),
                    hosting_config_mode = table.Column<int>(type: "integer", nullable: false),
                    charge_backers_export_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dashboard", x => x.id);
                    table.ForeignKey(
                        name: "fk_dashboard_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dashboard_user_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dashboard_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_claim",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: false),
                    claim_value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claim", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claim_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "change_set_entry",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: false),
                    change_type = table.Column<int>(type: "integer", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_change_set_entry", x => x.id);
                    table.ForeignKey(
                        name: "fk_change_set_entry_change_set_change_set_id",
                        column: x => x.ChangeSetId,
                        principalSchema: "audit",
                        principalTable: "change_set",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "charge_backer",
                schema: "charge_backers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    exported_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_fingerprints = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_charge_backer", x => x.id);
                    table.ForeignKey(
                        name: "fk_charge_backer_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discord_embed_web_hook_binding",
                schema: "embeds",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    webhook_url = table.Column<string>(type: "text", nullable: false),
                    message_template = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discord_embed_web_hook_binding", x => x.id);
                    table.ForeignKey(
                        name: "fk_discord_embed_web_hook_binding_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form",
                schema: "forms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    published_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    settings_limit_to_single_response = table.Column<bool>(type: "boolean", nullable: false),
                    settings_allow_access_to_results = table.Column<bool>(type: "boolean", nullable: false),
                    theme_theme_color = table.Column<string>(type: "text", nullable: true),
                    theme_header_picture_src = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_form_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "member",
                schema: "products",
                columns: table => new
                {
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    joined_at = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member", x => new { x.dashboard_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_member_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_member_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "member_role",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    name = table.Column<string>(type: "text", nullable: false),
                    permissions = table.Column<string>(type: "jsonb", nullable: false),
                    salary = table.Column<decimal>(type: "numeric", nullable: true),
                    currency = table.Column<int>(type: "integer", nullable: true),
                    payout_frequency = table.Column<int>(type: "integer", nullable: true),
                    color_hex = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_member_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_member_role_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_member_role_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_member_role_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_transaction",
                schema: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_tx_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_transaction_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_transaction_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_transaction_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_transaction_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan",
                schema: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    subscription_plan = table.Column<string>(type: "text", nullable: true),
                    license_life = table.Column<Duration>(type: "interval", nullable: true),
                    trial_period = table.Column<Duration>(type: "interval", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    unbindable_delay = table.Column<Duration>(type: "interval", nullable: true),
                    is_trial = table.Column<bool>(type: "boolean", nullable: false),
                    discord_role_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    protect_purchases_with_captcha = table.Column<bool>(type: "boolean", nullable: false),
                    format = table.Column<int>(type: "integer", nullable: false),
                    template = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plan_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plan_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "published_web_hook",
                schema: "web_hooks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    payload_data = table.Column<string>(type: "text", nullable: false),
                    payload_signature = table.Column<string>(type: "text", nullable: false),
                    payload_event_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    status_description = table.Column<string>(type: "text", nullable: true),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    listener_endpoint = table.Column<string>(type: "text", nullable: false),
                    timestamp = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_published_web_hook", x => x.id);
                    table.ForeignKey(
                        name: "fk_published_web_hook_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_session",
                schema: "analytics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    last_activity_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: false),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_session", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_session_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_session_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "web_hook_binding",
                schema: "web_hooks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    listener_endpoint = table.Column<string>(type: "text", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transport = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_web_hook_binding", x => x.id);
                    table.ForeignKey(
                        name: "fk_web_hook_binding_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "web_hooks_config",
                schema: "web_hooks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    client_secret = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_web_hooks_config", x => x.id);
                    table.ForeignKey(
                        name: "fk_web_hooks_config_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_component",
                schema: "forms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    name_value = table.Column<string>(type: "text", nullable: false),
                    name_picture_src = table.Column<string>(type: "text", nullable: true),
                    form_id = table.Column<long>(type: "bigint", nullable: false),
                    order = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: true),
                    section_id = table.Column<long>(type: "bigint", nullable: true),
                    options = table.Column<string>(type: "jsonb", nullable: true),
                    rows = table.Column<string>(type: "jsonb", nullable: true),
                    columns = table.Column<string>(type: "jsonb", nullable: true),
                    min = table.Column<byte>(type: "smallint", nullable: true),
                    max = table.Column<byte>(type: "smallint", nullable: true),
                    min_label = table.Column<string>(type: "text", nullable: true),
                    max_label = table.Column<string>(type: "text", nullable: true),
                    placeholder = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_component", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_component_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_component_form_component_section_id",
                        column: x => x.section_id,
                        principalSchema: "forms",
                        principalTable: "form_component",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_component_form_form_id",
                        column: x => x.form_id,
                        principalSchema: "forms",
                        principalTable: "form",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_component_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_form_component_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "form_response",
                schema: "forms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    form_id = table.Column<long>(type: "bigint", nullable: false),
                    responded_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_response", x => x.id);
                    table.ForeignKey(
                        name: "fk_form_response_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_response_form_form_id",
                        column: x => x.form_id,
                        principalSchema: "forms",
                        principalTable: "form",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_response_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_form_response_user_responded_by",
                        column: x => x.responded_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_response_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_member_role_binding",
                schema: "security",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    member_role_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    last_paid_out_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    remote_account_id = table.Column<string>(type: "text", nullable: true),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_member_role_binding", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_member_role_binding_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_member_role_binding_member_role_member_role_id",
                        column: x => x.member_role_id,
                        principalSchema: "security",
                        principalTable: "member_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_member_role_binding_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_member_role_binding_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_member_role_binding_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "release",
                schema: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    password = table.Column<string>(type: "text", nullable: false),
                    initial_stock = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "text", rowVersion: true, nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_release", x => x.id);
                    table.ForeignKey(
                        name: "fk_release_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_release_plan_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "products",
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_release_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_release_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "form_field_value",
                schema: "forms",
                columns: table => new
                {
                    field_id = table.Column<long>(type: "bigint", nullable: false),
                    FormResponseId = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_field_value", x => new { x.field_id, x.FormResponseId });
                    table.ForeignKey(
                        name: "fk_form_field_value_form_component_field_id",
                        column: x => x.field_id,
                        principalSchema: "forms",
                        principalTable: "form_component",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_form_field_value_form_response_form_response_id",
                        column: x => x.FormResponseId,
                        principalSchema: "forms",
                        principalTable: "form_response",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "license_key",
                schema: "products",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    release_id = table.Column<long>(type: "bigint", nullable: true),
                    expiry = table.Column<Instant>(type: "timestamp", nullable: true),
                    last_auth_request = table.Column<Instant>(type: "timestamp", nullable: true),
                    session_id = table.Column<string>(type: "text", nullable: true),
                    subscription_id = table.Column<string>(type: "text", nullable: true),
                    subscribed_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    subscription_cancelled_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    payment_intent = table.Column<string>(type: "text", nullable: true),
                    value = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    trial_ends_at = table.Column<Instant>(type: "timestamp", nullable: true),
                    unbindable_after = table.Column<Instant>(type: "timestamp", nullable: true),
                    suspensions = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    removed_at = table.Column<Instant>(type: "timestamp", nullable: false),
                    dashboard_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_key", x => x.id);
                    table.ForeignKey(
                        name: "fk_license_key_dashboard_dashboard_id",
                        column: x => x.dashboard_id,
                        principalSchema: "products",
                        principalTable: "dashboard",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_license_key_plan_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "products",
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_license_key_release_release_id",
                        column: x => x.release_id,
                        principalSchema: "products",
                        principalTable: "release",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_license_key_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_license_key_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_license_key_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_change_set_updated_by",
                schema: "audit",
                table: "change_set",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_change_set_entry_change_set_id",
                schema: "audit",
                table: "change_set_entry",
                column: "ChangeSetId");

            migrationBuilder.CreateIndex(
                name: "ix_charge_backer_dashboard_id",
                schema: "charge_backers",
                table: "charge_backer",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_created_by",
                schema: "products",
                table: "dashboard",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_hosting_config_mode_hosting_config_domain_name",
                schema: "products",
                table: "dashboard",
                columns: new[] { "hosting_config_mode", "hosting_config_domain_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_owner_id",
                schema: "products",
                table: "dashboard",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_removed_at",
                schema: "products",
                table: "dashboard",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_updated_by",
                schema: "products",
                table: "dashboard",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_discord_embed_web_hook_binding_dashboard_id",
                schema: "embeds",
                table: "discord_embed_web_hook_binding",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_created_by",
                schema: "forms",
                table: "form",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_form_dashboard_id",
                schema: "forms",
                table: "form",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_removed_at",
                schema: "forms",
                table: "form",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_form_updated_by",
                schema: "forms",
                table: "form",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_form_component_created_by",
                schema: "forms",
                table: "form_component",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_form_component_dashboard_id",
                schema: "forms",
                table: "form_component",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_component_form_id",
                schema: "forms",
                table: "form_component",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_component_removed_at",
                schema: "forms",
                table: "form_component",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_form_component_section_id",
                schema: "forms",
                table: "form_component",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_component_updated_by",
                schema: "forms",
                table: "form_component",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_form_field_value_form_response_id",
                schema: "forms",
                table: "form_field_value",
                column: "FormResponseId");

            migrationBuilder.CreateIndex(
                name: "ix_form_response_created_by",
                schema: "forms",
                table: "form_response",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_form_response_dashboard_id",
                schema: "forms",
                table: "form_response",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_response_form_id",
                schema: "forms",
                table: "form_response",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_response_removed_at",
                schema: "forms",
                table: "form_response",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_form_response_responded_by",
                schema: "forms",
                table: "form_response",
                column: "responded_by");

            migrationBuilder.CreateIndex(
                name: "ix_form_response_updated_by",
                schema: "forms",
                table: "form_response",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_created_by",
                schema: "products",
                table: "license_key",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_dashboard_id",
                schema: "products",
                table: "license_key",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_plan_id",
                schema: "products",
                table: "license_key",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_release_id",
                schema: "products",
                table: "license_key",
                column: "release_id");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_removed_at",
                schema: "products",
                table: "license_key",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_updated_by",
                schema: "products",
                table: "license_key",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_license_key_user_id",
                schema: "products",
                table: "license_key",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_user_id",
                schema: "products",
                table: "member",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_role_created_by",
                schema: "security",
                table: "member_role",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_member_role_dashboard_id",
                schema: "security",
                table: "member_role",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_member_role_name_dashboard_id",
                schema: "security",
                table: "member_role",
                columns: new[] { "name", "dashboard_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_member_role_removed_at",
                schema: "security",
                table: "member_role",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_member_role_updated_by",
                schema: "security",
                table: "member_role",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_created_by",
                schema: "orders",
                table: "payment_transaction",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_dashboard_id",
                schema: "orders",
                table: "payment_transaction",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_updated_by",
                schema: "orders",
                table: "payment_transaction",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_payment_transaction_user_id",
                schema: "orders",
                table: "payment_transaction",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_created_by",
                schema: "products",
                table: "plan",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_plan_dashboard_id",
                schema: "products",
                table: "plan",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_removed_at",
                schema: "products",
                table: "plan",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_plan_updated_by",
                schema: "products",
                table: "plan",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_published_web_hook_dashboard_id",
                schema: "web_hooks",
                table: "published_web_hook",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_release_created_by",
                schema: "products",
                table: "release",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_release_dashboard_id",
                schema: "products",
                table: "release",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_release_plan_id",
                schema: "products",
                table: "release",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_release_removed_at",
                schema: "products",
                table: "release",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_release_updated_by",
                schema: "products",
                table: "release",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                schema: "identity",
                table: "role",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email_index",
                schema: "identity",
                table: "user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_user_created_by",
                schema: "identity",
                table: "user",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_removed_at",
                schema: "identity",
                table: "user",
                column: "removed_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_updated_by",
                schema: "identity",
                table: "user",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                schema: "identity",
                table: "user",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claim_user_id",
                schema: "identity",
                table: "user_claim",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_member_role_binding_created_by",
                schema: "security",
                table: "user_member_role_binding",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_member_role_binding_dashboard_id",
                schema: "security",
                table: "user_member_role_binding",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_member_role_binding_member_role_id",
                schema: "security",
                table: "user_member_role_binding",
                column: "member_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_member_role_binding_updated_by",
                schema: "security",
                table: "user_member_role_binding",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_member_role_binding_user_id",
                schema: "security",
                table: "user_member_role_binding",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                schema: "identity",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_dashboard_id",
                schema: "analytics",
                table: "user_session",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_user_id",
                schema: "analytics",
                table: "user_session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_web_hook_binding_dashboard_id",
                schema: "web_hooks",
                table: "web_hook_binding",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "ix_web_hooks_config_dashboard_id",
                schema: "web_hooks",
                table: "web_hooks_config",
                column: "dashboard_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "change_set_entry",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "charge_backer",
                schema: "charge_backers");

            migrationBuilder.DropTable(
                name: "discord_embed_web_hook_binding",
                schema: "embeds");

            migrationBuilder.DropTable(
                name: "form_field_value",
                schema: "forms");

            migrationBuilder.DropTable(
                name: "license_key",
                schema: "products");

            migrationBuilder.DropTable(
                name: "member",
                schema: "products");

            migrationBuilder.DropTable(
                name: "payment_transaction",
                schema: "orders");

            migrationBuilder.DropTable(
                name: "published_web_hook",
                schema: "web_hooks");

            migrationBuilder.DropTable(
                name: "user_claim",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_member_role_binding",
                schema: "security");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_session",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "web_hook_binding",
                schema: "web_hooks");

            migrationBuilder.DropTable(
                name: "web_hooks_config",
                schema: "web_hooks");

            migrationBuilder.DropTable(
                name: "change_set",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "form_component",
                schema: "forms");

            migrationBuilder.DropTable(
                name: "form_response",
                schema: "forms");

            migrationBuilder.DropTable(
                name: "release",
                schema: "products");

            migrationBuilder.DropTable(
                name: "member_role",
                schema: "security");

            migrationBuilder.DropTable(
                name: "role",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "form",
                schema: "forms");

            migrationBuilder.DropTable(
                name: "plan",
                schema: "products");

            migrationBuilder.DropTable(
                name: "dashboard",
                schema: "products");

            migrationBuilder.DropTable(
                name: "user",
                schema: "identity");

            migrationBuilder.DropSequence(
                name: "charge_backer_hi_lo_sequence",
                schema: "charge_backers");

            migrationBuilder.DropSequence(
                name: "discord_embed_web_hook_binding_hi_lo_sequence",
                schema: "embeds");

            migrationBuilder.DropSequence(
                name: "form_component_hi_lo_sequence",
                schema: "forms");

            migrationBuilder.DropSequence(
                name: "form_hi_lo_sequence",
                schema: "forms");

            migrationBuilder.DropSequence(
                name: "form_response_hi_lo_sequence",
                schema: "forms");

            migrationBuilder.DropSequence(
                name: "license_key_hi_lo_sequence",
                schema: "products");

            migrationBuilder.DropSequence(
                name: "member_role_hi_lo_sequence",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "plan_hi_lo_sequence",
                schema: "products");

            migrationBuilder.DropSequence(
                name: "published_web_hook_hi_lo_sequence",
                schema: "web_hooks");

            migrationBuilder.DropSequence(
                name: "release_hi_lo_sequence",
                schema: "products");

            migrationBuilder.DropSequence(
                name: "role_hi_lo_sequence",
                schema: "identity");

            migrationBuilder.DropSequence(
                name: "user_claim_hi_lo_sequence",
                schema: "identity");

            migrationBuilder.DropSequence(
                name: "user_hi_lo_sequence",
                schema: "identity");

            migrationBuilder.DropSequence(
                name: "user_member_role_binding_hi_lo_sequence",
                schema: "security");

            migrationBuilder.DropSequence(
                name: "web_hook_binding_hi_lo_sequence",
                schema: "web_hooks");
        }
    }
}
