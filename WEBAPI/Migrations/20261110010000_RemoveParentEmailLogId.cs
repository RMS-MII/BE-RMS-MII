using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEBAPI.Migrations;

public partial class RemoveParentEmailLogId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The first retry migration may already have run before the retry design changed.
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'EmailLog', N'ParentEmailLogId') IS NOT NULL
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EmailLog_EmailLog_ParentEmailLogId'
                    AND parent_object_id = OBJECT_ID(N'EmailLog'))
                    ALTER TABLE [EmailLog] DROP CONSTRAINT [FK_EmailLog_EmailLog_ParentEmailLogId];

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailLog_ParentEmailLogId'
                    AND object_id = OBJECT_ID(N'EmailLog'))
                    DROP INDEX [IX_EmailLog_ParentEmailLogId] ON [EmailLog];

                ALTER TABLE [EmailLog] DROP COLUMN [ParentEmailLogId];
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The removed column is no longer part of the EmailLog model.
    }
}
