using HtmlTemplater.Domain.Dtos;
using System.Text.Json.Serialization;

namespace HtmlTemplater.Domain
{
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(ManifestDto))]
    [JsonSerializable(typeof(AssetsDto))]
    public partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
