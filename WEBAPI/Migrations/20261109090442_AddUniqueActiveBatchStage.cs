using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueActiveBatchStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BatchStage_RecruitmentBatchId",
                table: "BatchStage",
                column: "RecruitmentBatchId",
                unique: true,
                filter: "[LifecycleStatus] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BatchStage_RecruitmentBatchId",
                table: "BatchStage");
        }
    }
}
