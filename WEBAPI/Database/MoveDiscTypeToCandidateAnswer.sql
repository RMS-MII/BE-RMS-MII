-- Run against the recruitment database after backing it up.
-- Memindahkan metadata DISC dari relasi ke Question menjadi snapshot pada jawaban kandidat:
--   1) Menambahkan kolom DiscType pada CandidateAnswer.
--   2) Mengisi (backfill) DiscType dari Question.DiscType melalui QuestionId yang ada.
--   3) Menghapus relasi/index/kolom QuestionId dari CandidateAnswer.
--
-- Setelah skrip ini dijalankan, perhitungan DISC tidak lagi bergantung pada tabel Question.
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.CandidateAnswer', N'U') IS NULL
        THROW 51101, 'CandidateAnswer table does not exist in the selected database.', 1;

    -- 1) Tambahkan kolom snapshot DiscType (0 = tidak diketahui / data lama tanpa soal).
    IF COL_LENGTH(N'dbo.CandidateAnswer', N'DiscType') IS NULL
        ALTER TABLE dbo.CandidateAnswer
            ADD DiscType int NOT NULL CONSTRAINT DF_CandidateAnswer_DiscType DEFAULT (0);

    -- 2) Backfill nilai DISC dari master soal untuk data historis yang masih punya QuestionId.
    IF COL_LENGTH(N'dbo.CandidateAnswer', N'QuestionId') IS NOT NULL
        UPDATE answer
        SET answer.DiscType = question.DiscType
        FROM dbo.CandidateAnswer AS answer
        INNER JOIN dbo.Question AS question
            ON question.Id = answer.QuestionId
        WHERE answer.QuestionId IS NOT NULL;

    -- 3) Lepaskan relasi ke Question, lalu hapus kolom QuestionId.
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_CandidateAnswer_Question_QuestionId')
        ALTER TABLE dbo.CandidateAnswer
            DROP CONSTRAINT FK_CandidateAnswer_Question_QuestionId;

    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_CandidateAnswer_QuestionId'
          AND object_id = OBJECT_ID(N'dbo.CandidateAnswer'))
        DROP INDEX IX_CandidateAnswer_QuestionId ON dbo.CandidateAnswer;

    IF COL_LENGTH(N'dbo.CandidateAnswer', N'QuestionId') IS NOT NULL
        ALTER TABLE dbo.CandidateAnswer
            DROP COLUMN QuestionId;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
