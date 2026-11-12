-- Run against the recruitment database after backing it up.
-- Menambahkan metadata DISC pada soal dan relasi jawaban kandidat ke soal.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Question', N'U') IS NULL
        THROW 51100, 'Question table does not exist in the selected database.', 1;

    IF OBJECT_ID(N'dbo.CandidateAnswer', N'U') IS NULL
        THROW 51101, 'CandidateAnswer table does not exist in the selected database.', 1;

    IF COL_LENGTH(N'dbo.Question', N'DiscType') IS NULL
        ALTER TABLE dbo.Question
            ADD DiscType int NOT NULL CONSTRAINT DF_Question_DiscType DEFAULT (0);

    IF COL_LENGTH(N'dbo.CandidateAnswer', N'QuestionId') IS NULL
        ALTER TABLE dbo.CandidateAnswer
            ADD QuestionId uniqueidentifier NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_CandidateAnswer_QuestionId'
          AND object_id = OBJECT_ID(N'dbo.CandidateAnswer'))
        CREATE INDEX IX_CandidateAnswer_QuestionId ON dbo.CandidateAnswer (QuestionId);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_CandidateAnswer_Question_QuestionId')
        ALTER TABLE dbo.CandidateAnswer
            ADD CONSTRAINT FK_CandidateAnswer_Question_QuestionId
            FOREIGN KEY (QuestionId) REFERENCES dbo.Question (Id) ON DELETE NO ACTION;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
