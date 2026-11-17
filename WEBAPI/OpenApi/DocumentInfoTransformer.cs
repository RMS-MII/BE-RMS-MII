using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WEBAPI.OpenApi;

/// <summary>Document transformer 1: title, version, description, contact, license and (optional) servers.</summary>
public sealed class DocumentInfoTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Rekrutin API",
            Version = "v1",   // the version of YOUR API, not of the OpenAPI specification
            Description = "Rekrutin recruitment API for departments, positions, employees, and accounts.",
            Contact = new OpenApiContact
            {
                Name = "RMS Platform Team",
                Email = "rms-support@company.id",
                Url = new Uri("https://intranet.company.id/rms")
            },
            License = new OpenApiLicense
            {
                Name = "Internal use only",
                Url = new Uri("https://intranet.company.id/rms/license")
            }
        };

        // Optional: appsettings "OpenApi:Servers": [ { "Url": "https://hris.company.id", "Description": "Production" } ].
        // Without it the generator uses the address of the current request, which is what you want on localhost.
        // Swagger UI sends "Try it out" calls to the first server, so never list a URL whose port differs from launchSettings.json.
        var servers = configuration.GetSection("OpenApi:Servers").GetChildren()
            .Where(s => !string.IsNullOrWhiteSpace(s["Url"]))
            .Select(s => new OpenApiServer { Url = s["Url"]!, Description = s["Description"] })
            .ToList();
        if (servers.Count > 0)
            document.Servers = servers;

        return Task.CompletedTask;
    }
}
