using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLogDeliveryStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailLog_SentAt",
                table: "EmailLog");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "EmailLog",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptedAt",
                table: "EmailLog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "EmailLog",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EmailLog",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_AttemptedAt",
                table: "EmailLog",
                column: "AttemptedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailLog_AttemptedAt",
                table: "EmailLog");

            migrationBuilder.DropColumn(
                name: "AttemptedAt",
                table: "EmailLog");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "EmailLog");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmailLog");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "EmailLog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_SentAt",
                table: "EmailLog",
                column: "SentAt");
        }
    }
}
