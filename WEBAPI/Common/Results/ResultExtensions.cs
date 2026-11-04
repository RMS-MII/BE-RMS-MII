using Microsoft.AspNetCore.Mvc;

namespace WEBAPI.Common.Results;

public static class ResultExtensions
{
    public static IActionResult ToProblemDetails(this Result result, ControllerBase controller)
        => CreateProblem(result.Error, controller);

    public static IActionResult ToProblemDetails<T>(this Result<T> result, ControllerBase controller)
        => CreateProblem(result.Error, controller);

    private static IActionResult CreateProblem(Error error, ControllerBase controller)
    {
        var problem = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Description,
            Instance = controller.Request.Path
        };
        problem.Extensions["errorCode"] = error.Code;

        return new ObjectResult(problem) { StatusCode = error.StatusCode };
    }
}
