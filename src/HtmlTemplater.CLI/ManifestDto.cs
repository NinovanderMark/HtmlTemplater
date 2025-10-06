namespace HtmlTemplater.CLI
{
    public record ManifestDto
    {
        public List<string> Elements { get; init; } = [];
        public string? OutputPath { get; init; }
        public AssetsDto? Assets { get; init; }
    }

    public record AssetsDto
    {
        public string? Input { get; init; }
        public string? Output { get; init; }
    }
}
