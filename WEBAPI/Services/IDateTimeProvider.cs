namespace WEBAPI.Services;

public interface IDateTimeProvider
{
    DateTimeOffset GetUtcNow();
}