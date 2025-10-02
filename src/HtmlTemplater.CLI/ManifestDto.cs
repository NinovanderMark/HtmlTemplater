namespace HtmlTemplater.CLI
{
    public record ManifestDto
    {
        public List<string> Elements { get; init; } = [];
        public string? OutputPath { get; init; }
        public string? AssetFolder { get; init; }
    }
}
