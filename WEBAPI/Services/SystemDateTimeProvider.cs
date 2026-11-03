namespace WEBAPI.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    private readonly TimeProvider _timeProvider;

    public SystemDateTimeProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTimeOffset GetUtcNow() => _timeProvider.GetUtcNow();
}