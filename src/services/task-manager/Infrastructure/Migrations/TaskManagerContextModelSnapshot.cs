﻿// <auto-generated />
using System;
using Centurion.TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Centurion.TaskManager.Infrastructure.Migrations
{
    [DbContext(typeof(TaskManagerContext))]
    partial class TaskManagerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Centurion.TaskManager.Core.CheckoutTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("CheckoutProxyPoolId")
                        .HasColumnType("uuid")
                        .HasColumnName("checkout_proxy_pool_id");

                    b.Property<byte[]>("Config")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("config");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid")
                        .HasColumnName("group_id");

                    b.Property<string>("Module")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("module");

                    b.Property<Guid?>("MonitorProxyPoolId")
                        .HasColumnType("uuid")
                        .HasColumnName("monitor_proxy_pool_id");

                    b.Property<string>("ProductSku")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("product_sku");

                    b.Property<string>("ProfileIds")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("jsonb")
                        .HasColumnName("profile_ids")
                        .HasDefaultValueSql("'[]'");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_checkout_task");

                    b.HasIndex("GroupId")
                        .HasDatabaseName("ix_checkout_task_group_id");

                    b.ToTable("checkout_task", "public");
                });

            modelBuilder.Entity("Centurion.TaskManager.Core.CheckoutTaskGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_checkout_task_group");

                    b.ToTable("checkout_task_group", "public");
                });

            modelBuilder.Entity("Centurion.TaskManager.Core.Events.ProductCheckedOutEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Account")
                        .HasColumnType("text")
                        .HasColumnName("account");

                    b.Property<string>("Attrs")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("attrs");

                    b.Property<Duration>("Delay")
                        .HasColumnType("interval")
                        .HasColumnName("delay");

                    b.Property<Duration>("Duration")
                        .HasColumnType("interval")
                        .HasColumnName("duration");

                    b.Property<string>("FormattedPrice")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("formatted_price");

                    b.Property<string>("Links")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("links");

                    b.Property<string>("Mode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("mode");

                    b.Property<string>("Picture")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("picture");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric")
                        .HasColumnName("price");

                    b.Property<string>("ProcessingLog")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("processing_log");

                    b.Property<string>("Profile")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("profile");

                    b.Property<string>("Proxy")
                        .HasColumnType("text")
                        .HasColumnName("proxy");

                    b.Property<long>("Qty")
                        .HasColumnType("bigint")
                        .HasColumnName("qty");

                    b.Property<string>("ShopIconUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("shop_icon_url");

                    b.Property<string>("ShopTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("shop_title");

                    b.Property<string>("Sku")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("sku");

                    b.Property<string>("Store")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("store");

                    b.Property<string>("TaskId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("task_id");

                    b.Property<string>("Thumbnail")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("thumbnail");

                    b.Property<Instant>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("url");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_product_checked_out_event");

                    b.ToTable("product_checked_out_event", "events");
                });

            modelBuilder.Entity("Centurion.TaskManager.Core.Presets.Preset", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<byte[]>("Config")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("config");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text")
                        .HasColumnName("created_by");

                    b.Property<Instant>("ExpectedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expected_at");

                    b.Property<string>("Module")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("module");

                    b.Property<string>("ProductPicture")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("product_picture");

                    b.Property<string>("ProductSku")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("product_sku");

                    b.Property<string>("ProductTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("product_title");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("updated_by");

                    b.HasKey("Id")
                        .HasName("pk_preset");

                    b.ToTable("preset", "public");
                });

            modelBuilder.Entity("Centurion.TaskManager.Core.Product", b =>
                {
                    b.Property<string>("Module")
                        .HasColumnType("text")
                        .HasColumnName("module");

                    b.Property<string>("Sku")
                        .HasColumnType("text")
                        .HasColumnName("sku");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("image");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("link");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<decimal?>("Price")
                        .HasColumnType("numeric")
                        .HasColumnName("price");

                    b.HasKey("Module", "Sku")
                        .HasName("pk_product");

                    b.ToTable("product", "public");
                });

            modelBuilder.Entity("Centurion.TaskManager.Core.CheckoutTask", b =>
                {
                    b.HasOne("Centurion.TaskManager.Core.CheckoutTaskGroup", null)
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_checkout_task_checkout_task_group_group_id");
                });
#pragma warning restore 612, 618
        }
    }
}
