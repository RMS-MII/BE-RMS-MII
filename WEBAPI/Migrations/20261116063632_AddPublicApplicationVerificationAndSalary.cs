using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicApplicationVerificationAndSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PlannedDurationTicks",
                table: "BatchStage",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedSalary",
                table: "BatchApplications",
                type: "money",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PublicApplicationVerification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicApplicationVerification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicApplicationVerification_RecruitmentBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "RecruitmentBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicApplicationVerification_BatchId",
                table: "PublicApplicationVerification",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicApplicationVerification_Email_CreatedAt",
                table: "PublicApplicationVerification",
                columns: new[] { "Email", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicApplicationVerification");

            migrationBuilder.DropColumn(
                name: "PlannedDurationTicks",
                table: "BatchStage");

            migrationBuilder.DropColumn(
                name: "ExpectedSalary",
                table: "BatchApplications");
        }
    }
}
