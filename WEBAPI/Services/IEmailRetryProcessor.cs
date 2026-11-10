namespace WEBAPI.Services;

public interface IEmailRetryProcessor
{
    Task<bool> ProcessNextPendingAsync(CancellationToken cancellationToken);
}
