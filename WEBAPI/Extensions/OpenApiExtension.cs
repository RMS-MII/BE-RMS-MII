using WEBAPI.OpenApi;

namespace WEBAPI.Extensions;

public static class OpenApiExtensions
{
    /// <summary>
    ///  Module 12: the built-in OpenAPI generator (Microsoft.AspNetCore.OpenApi) with the HRIS customizations.
    /// </summary>
    public static IServiceCollection AddRmsOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<DocumentInfoTransformer>();
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<AuthorizeOperationTransformer>();
        });
        return services;
    }
    // created by dependency injection, so it can take IConfiguration

    public static WebApplication UseRmsOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapOpenApi().AllowAnonymous();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "RMS API"));
        return app;
    }
    
}