using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Features.Batches.Commands.ActivateBatchStage;
using WEBAPI.Services;

var config = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("WEBAPI/appsettings.Development.json")).Build();
var probe = new ActivationProbe();
var options = new DbContextOptionsBuilder<RmsDbContext>()
    .UseSqlServer(config.GetConnectionString("RmsDB")).AddInterceptors(probe).Options;
await using var db = new RmsDbContext(options);
var handler = new ActivateBatchStageHandler(db, new SystemDateTimeProvider(TimeProvider.System), NullLogger<ActivateBatchStageHandler>.Instance);
try
{
    var result = await handler.Handle(new ActivateBatchStageCommand(Guid.Parse(args[0]), Guid.Parse(args[1])), default);
    throw new Exception(result.IsFailure ? result.Error.ToString() : "No new stages were checked.");
}
catch (ProbeComplete)
{
    Console.WriteLine("PASS: activation tracks all new candidate stages as Added. Save was intercepted; no data written.");
}

sealed class ProbeComplete : Exception;
sealed class ActivationProbe : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var entries = eventData.Context!.ChangeTracker.Entries<ApplicationStage>()
            .Where(e => e.State != EntityState.Unchanged).ToList();
        if (entries.Count == 0) throw new Exception("No candidate stages created.");
        if (entries.Any(e => e.State != EntityState.Added))
            throw new Exception("New stages incorrectly tracked: " + string.Join(", ", entries.Select(e => e.State)));
        throw new ProbeComplete();
    }
}

