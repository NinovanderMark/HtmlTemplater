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
    public partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
