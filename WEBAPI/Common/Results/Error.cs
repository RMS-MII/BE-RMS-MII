namespace WEBAPI.Common.Results;

public sealed record Error(string Code, string Description, int StatusCode)
{
    public static Error Validation(string description)
        => new("VALIDATION_ERROR", description, StatusCodes.Status400BadRequest);

    public static Error Unauthorized(string description)
        => new("UNAUTHENTICATED", description, StatusCodes.Status401Unauthorized);

    public static Error Forbidden(string description)
        => new("FORBIDDEN", description, StatusCodes.Status403Forbidden);

    public static Error NotFound(string description)
        => new("NOT_FOUND", description, StatusCodes.Status404NotFound);

    public static Error Conflict(string description)
        => new("CONFLICT", description, StatusCodes.Status409Conflict);

    public static Error BusinessRule(string description)
        => new("BUSINESS_RULE_VIOLATION", description, StatusCodes.Status422UnprocessableEntity);

    public string Title => StatusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Resource not found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Business rule violated",
        _ => "Error"
    };
}
