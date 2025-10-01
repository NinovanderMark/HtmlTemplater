using HtmlTemplater.Domain;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HtmlTemplater.Parsing
{
    public class AgilityParser(ILogger<AgilityParser> _logger)
    {
        private readonly SynchronizedCollection<Element> _elements = [];

        public void AddElements(List<Element> elements)
        {
            elements.ForEach(_elements.Add);
        }

        public async Task ParsePage(Page page, string outputFolder)
        {
            // Parse page HTML
            // Replace element placeholders with element definitions
            // Replace {{ InnerHtml }} in element definitions with page content
            // Write parsed HTML to output folder
        }
    }
}
