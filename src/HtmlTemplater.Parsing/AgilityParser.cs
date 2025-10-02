using HtmlAgilityPack;
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

        /// <summary>
        /// Parses the page, replacing element placeholders with their definitions.
        /// </summary>
        /// <param name="page">The page to parse</param>
        /// <returns>The parsed page</returns>
        public async Task<Page> ParsePage(Page page)
        {
            var document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(page.Content);

            // Replace element placeholders with element definitions
            foreach (var el in _elements)
            {
                var nodes = document.DocumentNode.SelectNodes(el.Name.ToLower()) ?? new HtmlAgilityPack.HtmlNodeCollection(document.DocumentNode);
                foreach(var node in nodes)
                {
                    var innerHtml = node.InnerHtml;
                    var replaceable = GetReplaceable(el);
                    if ( replaceable != null && innerHtml != null)
                    {
                        // Replace element placeholder, and use its inner HTML instead of the definition's {{ InnerHtml }} 
                        var newNode = HtmlNode.CreateNode(el.Html.Replace(replaceable, innerHtml));
                        node.ParentNode.ReplaceChild(newNode, node);
                        continue;
                    }
                    else if (replaceable != null && innerHtml == null)
                    {
                        _logger.LogWarning("{PageName}:{Line},{Column}; No inner HTML provided for element '{ElementName}'",
                            page.Name, node.Line, node.LinePosition, el.Name);

                        // Replace element placeholder, removing the {{ InnerHtml }} token
                        var newNode = HtmlNode.CreateNode(el.Html.Replace(replaceable, string.Empty));
                        node.ParentNode.ReplaceChild(newNode, node);
                        continue;
                    }

                    if ( innerHtml != null)
                    {
                        _logger.LogWarning("{PageName}:{Line},{Column}; Inner HTML provided for element '{ElementName}' without replaceable token",
                            page.Name, node.Line, node.LinePosition, el.Name);
                    }

                    // Just replace the element placeholder with the element definition
                    var newNode2 = HtmlNode.CreateNode(el.Html);
                    node.ParentNode.ReplaceChild(newNode2, node);                    
                }
            }

            return new Page(page.Name, page.Path, document.DocumentNode.OuterHtml);
        }

        private string? GetReplaceable(Element el)
        {
            int start = el.Html.IndexOf("{{");
            int end = el.Html.IndexOf("}}") + "}}".Length;
            string replaceable = el.Html[start..end];
            if ( replaceable.Contains("InnerHtml") )
            {
                return replaceable;
            }

            return null;
        }
    }
}
