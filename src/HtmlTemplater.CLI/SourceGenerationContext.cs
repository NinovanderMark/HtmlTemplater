using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HtmlTemplater.CLI
{
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(ManifestDto))]
    [JsonSerializable(typeof(AssetsDto))]
    public partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
