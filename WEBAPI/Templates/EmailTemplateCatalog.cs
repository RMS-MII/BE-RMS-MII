using System.Net;
using System.Text.Json;

namespace WEBAPI.Templates;

public static class EmailTemplateCatalog
{
    private const string LayoutFileName = "EmailLayout.html";
    private const string TemplatesFileName = "EmailTemplates.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly Lazy<IReadOnlyDictionary<string, EmailTemplateDefinition>> Templates =
        new(LoadTemplates);

    private static readonly Lazy<string> Layout = new(LoadLayout);

    public static EmailTemplateDefinition Get(string templateName)
    {
        var templates = Templates.Value;

        if (templates.TryGetValue(templateName, out var definition))
            return definition;

        throw new KeyNotFoundException(
            $"Email template definition '{templateName}' was not found in '{TemplatesFileName}'.");
    }

    public static string RenderLayout(string title, string contentHtml)
    {
        return Layout.Value
            .Replace("{{title}}", WebUtility.HtmlEncode(title))
            .Replace("{{contentHtml}}", contentHtml);
    }

    private static IReadOnlyDictionary<string, EmailTemplateDefinition> LoadTemplates()
    {
        var path = ResolveTemplatePath(TemplatesFileName);

        using var stream = File.OpenRead(path);
        var templates = JsonSerializer.Deserialize<Dictionary<string, EmailTemplateDefinition>>(
            stream,
            JsonOptions);

        if (templates is null || templates.Count == 0)
            throw new InvalidOperationException(
                $"Email template catalog '{path}' is empty or invalid.");

        return new Dictionary<string, EmailTemplateDefinition>(templates, StringComparer.OrdinalIgnoreCase);
    }

    private static string LoadLayout()
    {
        var path = ResolveTemplatePath(LayoutFileName);
        var content = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException($"Email layout template '{path}' is empty.");

        return content;
    }

    private static string ResolveTemplatePath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Templates", "Emails", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "WEBAPI", "Templates", "Emails", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "WEBAPI", "Templates", "Emails", fileName)
        };

        var existingPath = candidates.FirstOrDefault(File.Exists);
        if (existingPath is not null)
            return existingPath;

        throw new FileNotFoundException(
            $"Email template file '{fileName}' was not found. Checked locations: {string.Join(", ", candidates)}");
    }
}
