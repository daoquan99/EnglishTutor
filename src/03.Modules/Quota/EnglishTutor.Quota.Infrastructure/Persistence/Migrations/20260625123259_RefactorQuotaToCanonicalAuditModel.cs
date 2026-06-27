using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishTutor.Quota.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorQuotaToCanonicalAuditModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. user_quota_states ---
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "quota",
                table: "user_quota_states",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "quota",
                table: "user_quota_states",
                newName: "updated_at_utc");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at_utc",
                schema: "quota",
                table: "user_quota_states",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "quota",
                table: "user_quota_states");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "quota",
                table: "user_quota_states");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "quota",
                table: "user_quota_states",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "quota",
                table: "user_quota_states",
                type: "uuid",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "quota",
                table: "user_quota_states",
                newName: "is_deleted");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "quota",
                table: "user_quota_states",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "quota",
                table: "user_quota_states",
                newName: "deleted_at_utc");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "quota",
                table: "user_quota_states");

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by_user_id",
                schema: "quota",
                table: "user_quota_states",
                type: "uuid",
                nullable: true);


            // --- 2. user_quota_rules ---
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "quota",
                table: "user_quota_rules",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "quota",
                table: "user_quota_rules",
                newName: "updated_at_utc");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at_utc",
                schema: "quota",
                table: "user_quota_rules",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "quota",
                table: "user_quota_rules");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "quota",
                table: "user_quota_rules");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "quota",
                table: "user_quota_rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "quota",
                table: "user_quota_rules",
                type: "uuid",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "quota",
                table: "user_quota_rules",
                newName: "is_deleted");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "quota",
                table: "user_quota_rules",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "quota",
                table: "user_quota_rules",
                newName: "deleted_at_utc");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "quota",
                table: "user_quota_rules");

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by_user_id",
                schema: "quota",
                table: "user_quota_rules",
                type: "uuid",
                nullable: true);


            // --- 3. quota_reservations ---
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "quota",
                table: "quota_reservations",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "quota",
                table: "quota_reservations",
                newName: "updated_at_utc");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at_utc",
                schema: "quota",
                table: "quota_reservations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "quota",
                table: "quota_reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "quota",
                table: "quota_reservations");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "quota",
                table: "quota_reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "quota",
                table: "quota_reservations",
                type: "uuid",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "quota",
                table: "quota_reservations",
                newName: "is_deleted");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "quota",
                table: "quota_reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "quota",
                table: "quota_reservations",
                newName: "deleted_at_utc");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "quota",
                table: "quota_reservations");

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by_user_id",
                schema: "quota",
                table: "quota_reservations",
                type: "uuid",
                nullable: true);


            // --- 4. usage_logs ---
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "quota",
                table: "usage_logs",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "quota",
                table: "usage_logs",
                newName: "updated_at_utc");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at_utc",
                schema: "quota",
                table: "usage_logs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "quota",
                table: "usage_logs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "quota",
                table: "usage_logs");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "quota",
                table: "usage_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "quota",
                table: "usage_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "quota",
                table: "usage_logs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "quota",
                table: "usage_logs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "quota",
                table: "usage_logs");


            // --- 5. rate_limit_events ---
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "quota",
                table: "rate_limit_events",
                newName: "created_at_utc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "quota",
                table: "rate_limit_events",
                newName: "updated_at_utc");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at_utc",
                schema: "quota",
                table: "rate_limit_events",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "quota",
                table: "rate_limit_events");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "quota",
                table: "rate_limit_events");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                schema: "quota",
                table: "rate_limit_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                schema: "quota",
                table: "rate_limit_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "quota",
                table: "rate_limit_events");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "quota",
                table: "rate_limit_events");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "quota",
                table: "rate_limit_events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- 1. user_quota_states ---
            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "quota",
                table: "user_quota_states",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "quota",
                table: "user_quota_states",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "quota",
                table: "user_quota_states",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "quota",
                table: "user_quota_states",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "quota",
                table: "user_quota_states",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "quota",
                table: "user_quota_states");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "quota",
                table: "user_quota_states");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                schema: "quota",
                table: "user_quota_states",
                newName: "IsDeleted");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "quota",
                table: "user_quota_states",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.RenameColumn(
                name: "deleted_at_utc",
                schema: "quota",
                table: "user_quota_states",
                newName: "DeletedAt");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "quota",
                table: "user_quota_states",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "deleted_by_user_id",
                schema: "quota",
                table: "user_quota_states");


            // --- 2. user_quota_rules ---
            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "quota",
                table: "user_quota_rules",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "quota",
                table: "user_quota_rules",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "quota",
                table: "user_quota_rules",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "quota",
                table: "user_quota_rules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "quota",
                table: "user_quota_rules",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "quota",
                table: "user_quota_rules");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "quota",
                table: "user_quota_rules");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                schema: "quota",
                table: "user_quota_rules",
                newName: "IsDeleted");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "quota",
                table: "user_quota_rules",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.RenameColumn(
                name: "deleted_at_utc",
                schema: "quota",
                table: "user_quota_rules",
                newName: "DeletedAt");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "quota",
                table: "user_quota_rules",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "deleted_by_user_id",
                schema: "quota",
                table: "user_quota_rules");


            // --- 3. quota_reservations ---
            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "quota",
                table: "quota_reservations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "quota",
                table: "quota_reservations",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "quota",
                table: "quota_reservations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "quota",
                table: "quota_reservations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "quota",
                table: "quota_reservations",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "quota",
                table: "quota_reservations");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "quota",
                table: "quota_reservations");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                schema: "quota",
                table: "quota_reservations",
                newName: "IsDeleted");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "quota",
                table: "quota_reservations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.RenameColumn(
                name: "deleted_at_utc",
                schema: "quota",
                table: "quota_reservations",
                newName: "DeletedAt");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "quota",
                table: "quota_reservations",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "deleted_by_user_id",
                schema: "quota",
                table: "quota_reservations");


            // --- 4. usage_logs ---
            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "quota",
                table: "usage_logs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "quota",
                table: "usage_logs",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "quota",
                table: "usage_logs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "quota",
                table: "usage_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "quota",
                table: "usage_logs",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "quota",
                table: "usage_logs");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "quota",
                table: "usage_logs");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "quota",
                table: "usage_logs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "quota",
                table: "usage_logs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "quota",
                table: "usage_logs",
                type: "text",
                nullable: true);


            // --- 5. rate_limit_events ---
            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                schema: "quota",
                table: "rate_limit_events",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at_utc",
                schema: "quota",
                table: "rate_limit_events",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "quota",
                table: "rate_limit_events",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "quota",
                table: "rate_limit_events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "quota",
                table: "rate_limit_events",
                type: "text",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                schema: "quota",
                table: "rate_limit_events");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                schema: "quota",
                table: "rate_limit_events");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "quota",
                table: "rate_limit_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "quota",
                table: "rate_limit_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "quota",
                table: "rate_limit_events",
                type: "text",
                nullable: true);
        }
    }
}
