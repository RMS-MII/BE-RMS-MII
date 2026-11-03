using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Exceptions;

namespace WEBAPI.Common.ExceptionHandlers;

public class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not RmsException hrisException)
            return false;   // unknown exception: GlobalExceptionHandler returns a generic 500

        // The exception object is not passed to the logger: a 404 or 409 is an expected outcome,
        // so we write one Warning line without a stack trace.
        logger.LogWarning("Request rejected with {StatusCode} {ErrorCode} ({ExceptionType}): {Reason}",
            hrisException.StatusCode, hrisException.ErrorCode, hrisException.GetType().Name, hrisException.Message);

        var problem = new ProblemDetails
        {
            Status = hrisException.StatusCode,
            Title = hrisException.Title,
            Detail = hrisException.Message   // safe: we wrote this message ourselves
        };
        problem.Extensions[ProblemDetailsWriter.ErrorCodeKey] = hrisException.ErrorCode;

        await ProblemDetailsWriter.WriteAsync(problemDetailsService, httpContext, problem, exception, ct);
        return true;
    }
}