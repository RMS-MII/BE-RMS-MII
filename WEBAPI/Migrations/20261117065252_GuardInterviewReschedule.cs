using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class GuardInterviewReschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequestedByCandidate",
                table: "RescheduleRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Older rows only recorded email. Treat ambiguous employee/candidate addresses as internal.
            migrationBuilder.Sql("""
                UPDATE request SET RequestedByCandidate = 1
                FROM RescheduleRequest request
                JOIN InterviewSession session ON session.Id = request.InterviewSessionId
                JOIN ApplicationStages stage ON stage.Id = session.ApplicationStageId
                JOIN BatchApplications application ON application.Id = stage.BatchApplicationId
                JOIN Candidates candidate ON candidate.Id = application.CandidateId
                WHERE request.RequesterEmail = candidate.Email
                  AND NOT EXISTS (SELECT 1 FROM Employee employee WHERE employee.Email = request.RequesterEmail);

                WITH PendingRequests AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY InterviewSessionId ORDER BY CreatedAt, Id) AS Position
                    FROM RescheduleRequest WHERE Status = 1
                )
                UPDATE request
                SET Status = 3, ReviewNotes = N'Duplicate pending request archived during migration.',
                    ReviewedAt = SYSUTCDATETIME(), UpdatedAt = SYSUTCDATETIME()
                FROM RescheduleRequest request
                JOIN PendingRequests ranked ON request.Id = ranked.Id
                WHERE ranked.Position > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_RescheduleRequest_InterviewSessionId_Status",
                table: "RescheduleRequest",
                columns: new[] { "InterviewSessionId", "Status" },
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RescheduleRequest_InterviewSessionId_Status",
                table: "RescheduleRequest");

            migrationBuilder.DropColumn(
                name: "RequestedByCandidate",
                table: "RescheduleRequest");
        }
    }
}
