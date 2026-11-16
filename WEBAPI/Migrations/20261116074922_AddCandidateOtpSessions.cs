using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateOtpSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "BatchId",
                table: "PublicApplicationVerification",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "PublicApplicationVerification",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsInTalentPool",
                table: "Candidates",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sign-in challenges have no batch and cannot be used by the previous schema.
            migrationBuilder.Sql("DELETE FROM [PublicApplicationVerification] WHERE [BatchId] IS NULL");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "PublicApplicationVerification");

            migrationBuilder.DropColumn(
                name: "IsInTalentPool",
                table: "Candidates");

            migrationBuilder.AlterColumn<Guid>(
                name: "BatchId",
                table: "PublicApplicationVerification",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
