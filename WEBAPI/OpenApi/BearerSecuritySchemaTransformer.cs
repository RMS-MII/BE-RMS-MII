using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WEBAPI.OpenApi;

public class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeId = "Bearer";   // the name that the padlock transformer refers to
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",            // lowercase, as defined by RFC 6750
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the accessToken from POST /api/auth/login (without the 'Bearer ' prefix)."
        };
        return Task.CompletedTask;
    }
}