using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class AlignMergedExamAndInterviewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SessionInterview_InterviewSessionId",
                table: "SessionInterview");

            migrationBuilder.AddColumn<bool>(
                name: "IsLead",
                table: "SessionInterview",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptStartedAt",
                table: "ApplicationStages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_SessionInterview_OneLeadPerSession",
                table: "SessionInterview",
                column: "InterviewSessionId",
                unique: true,
                filter: "[IsLead] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_SessionInterview_OneLeadPerSession",
                table: "SessionInterview");

            migrationBuilder.DropColumn(
                name: "IsLead",
                table: "SessionInterview");

            migrationBuilder.DropColumn(
                name: "AttemptStartedAt",
                table: "ApplicationStages");

            migrationBuilder.CreateIndex(
                name: "IX_SessionInterview_InterviewSessionId",
                table: "SessionInterview",
                column: "InterviewSessionId");
        }
    }
}
