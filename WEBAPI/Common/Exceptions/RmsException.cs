namespace WEBAPI.Common.Exceptions;

public abstract class RmsException : Exception
{
    protected RmsException() { }
    protected RmsException(string message) : base(message) { }
    protected RmsException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>HTTP status code returned to the client.</summary>
    public abstract int StatusCode { get; }

    /// <summary>Short, stable ProblemDetails title.</summary>
    public abstract string Title { get; }

    /// <summary>Machine-readable code for client applications (ProblemDetails extension "errorCode").</summary>
    public virtual string ErrorCode => "RMS_ERROR";
}