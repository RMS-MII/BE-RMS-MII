using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeBackgroundWorkerQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmailLog_Status_AttemptedAt",
                table: "EmailLog",
                columns: new[] { "Status", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStages_Status_AttemptStartedAt",
                table: "ApplicationStages",
                columns: new[] { "Status", "AttemptStartedAt" },
                filter: "[IsAttempts] = 0 AND [AttemptStartedAt] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailLog_Status_AttemptedAt",
                table: "EmailLog");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStages_Status_AttemptStartedAt",
                table: "ApplicationStages");
        }
    }
}
