using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlTemplater.Domain.Services
{
    public interface ISiteGenerator
    {
        Task<int> GenerateFromManifest(string manifestPath);
    }
}
