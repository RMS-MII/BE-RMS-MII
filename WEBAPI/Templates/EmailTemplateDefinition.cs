namespace WEBAPI.Templates;

public sealed class EmailTemplateDefinition
{
    public string Subject { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Greeting { get; init; } = "Halo {{recipientName}},";
    public string Summary { get; init; } = string.Empty;
    public List<EmailTemplateDetailDefinition> Details { get; init; } = [];
    public string? ActionLabel { get; init; }
    public string? ActionUrl { get; init; }
    public string? Note { get; init; }
    public string Closing { get; init; } = "Salam,\nTim Rekrutmen";
}

public sealed class EmailTemplateDetailDefinition
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool SkipWhenEmpty { get; init; } = true;
}
