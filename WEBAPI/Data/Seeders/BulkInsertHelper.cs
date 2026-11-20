using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace WEBAPI.Data.Seeders;

/// <summary>
/// Helper SqlBulkCopy untuk tabel berukuran besar. Mapping kolom dilakukan eksplisit
/// agar konversi enum/DateOnly yang biasanya ditangani EF Core tetap benar.
/// </summary>
internal static class BulkInsertHelper
{
    public static async Task InsertAsync<T>(
        RmsDbContext context,
        string destinationTable,
        IReadOnlyList<T> rows,
        int batchSize,
        IReadOnlyList<(string Name, Type Type)> columns,
        Action<DataRow, T> fill,
        CancellationToken ct)
    {
        if (rows.Count == 0)
            return;

        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("SQL Server connection string is not available for SqlBulkCopy.");

        await using var bulkConnection = new SqlConnection(connectionString);
        await bulkConnection.OpenAsync(ct);

        try
        {
            using var bulkCopy = new SqlBulkCopy(
                bulkConnection,
                SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.CheckConstraints,
                null)
            {
                DestinationTableName = destinationTable,
                BatchSize = batchSize,
                BulkCopyTimeout = 300
            };

            foreach (var column in columns)
                bulkCopy.ColumnMappings.Add(column.Name, column.Name);

            for (var offset = 0; offset < rows.Count; offset += batchSize)
            {
                var chunkSize = Math.Min(batchSize, rows.Count - offset);
                using var table = CreateDataTable(rows, offset, chunkSize, columns, fill);
                await bulkCopy.WriteToServerAsync(table, ct);
            }
        }
        finally
        {
            await bulkConnection.CloseAsync();
        }
    }

    private static DataTable CreateDataTable<T>(
        IReadOnlyList<T> rows,
        int offset,
        int count,
        IReadOnlyList<(string Name, Type Type)> columns,
        Action<DataRow, T> fill)
    {
        var table = new DataTable();

        foreach (var column in columns)
        {
            var dataColumn = table.Columns.Add(column.Name, column.Type);
            dataColumn.AllowDBNull = true;
        }

        for (var index = offset; index < offset + count; index++)
        {
            var row = table.NewRow();
            fill(row, rows[index]);
            table.Rows.Add(row);
        }

        return table;
    }
}
