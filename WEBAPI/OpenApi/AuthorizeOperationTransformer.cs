using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WEBAPI.OpenApi;

/// <summary>
/// Operation transformer: REQUIREMENT. Adds the padlock to every operation EXCEPT those marked [AllowAnonymous].
/// The check is inverted on purpose: with the fallback policy from Module 10, an action without [Authorize] still needs a token.
/// </summary>
public sealed class AuthorizeOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var allowsAnonymous = context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
        if (allowsAnonymous)
            return Task.CompletedTask;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            // A reference (not a copy) to components/securitySchemes/Bearer; an empty list because a Bearer scheme has no scopes.
            [new OpenApiSecuritySchemeReference(BearerSecuritySchemeTransformer.SchemeId, context.Document)] = []
        });
        return Task.CompletedTask;
    }
}