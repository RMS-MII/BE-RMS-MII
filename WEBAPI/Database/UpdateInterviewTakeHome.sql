-- Run against the recruitment database after backing it up.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.InterviewSession', N'U') IS NULL
        THROW 51000, 'InterviewSession does not exist in the selected database.', 1;

    IF COL_LENGTH(N'dbo.InterviewSession', N'FileSubmission') IS NOT NULL
    BEGIN
        IF COL_LENGTH(N'dbo.InterviewSession', N'SubmissionFile') IS NOT NULL
            THROW 51001, 'Both submission columns exist. Reconcile them before running this script.', 1;

        EXEC sys.sp_rename N'dbo.InterviewSession.FileSubmission', N'SubmissionFile', N'COLUMN';
    END;

    IF COL_LENGTH(N'dbo.InterviewSession', N'SubmissionFile') IS NULL
        THROW 51002, 'No submission column exists. Check the current schema.', 1;

    IF COL_LENGTH(N'dbo.InterviewSession', N'FileTakeHome') IS NULL
        ALTER TABLE dbo.InterviewSession ADD FileTakeHome nvarchar(200) NULL;

    -- Compile after renaming and adding columns so SQL Server sees the new names.
    EXEC sys.sp_executesql N'
        IF EXISTS (SELECT 1 FROM dbo.InterviewSession
                   WHERE DATALENGTH(SubmissionFile) > 400 OR DATALENGTH(FileTakeHome) > 400)
            THROW 51003, ''A stored link exceeds 200 characters. Review it before updating.'', 1;

        ALTER TABLE dbo.InterviewSession ALTER COLUMN SubmissionFile nvarchar(200) NULL;
        ALTER TABLE dbo.InterviewSession ALTER COLUMN FileTakeHome nvarchar(200) NULL;
    ';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
