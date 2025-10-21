using HtmlAgilityPack;
using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Models;
using HtmlTemplater.Parsing.Agility.Interfaces;

namespace HtmlTemplater.Parsing.Agility.Services
{
    public class AgilityParser(IElementRepository _elementRepository, INodeParser _nodeParser) : IParser
    {
        /// <summary>
        /// Parses the page, replacing element placeholders with their definitions.
        /// </summary>
        /// <param name="page">The page to parse</param>
        /// <returns>The parsed page</returns>
        public Page ParsePage(Page page)
        {
            var document = new HtmlDocument();
            document.LoadHtml(page.Content);

            // Replace element placeholders with element definitions
            foreach (var el in _elementRepository.KnownElements)
            {
                var nodes = document.DocumentNode.SelectNodes(el.ToLower()) ?? new HtmlNodeCollection(document.DocumentNode);
                foreach(var node in nodes)
                {
                    try
                    {
                        var element = _elementRepository.GetRequired(el);
                        var newNode = _nodeParser.ParseNode(node, element, page.Name);
                        node.ParentNode.ReplaceChild(newNode, node);
                    }
                    catch (Exception ex)
                    {
                        throw new ParsingException(page.Name, node.Line, node.LinePosition, ex.Message, ex);
                    }
                }
            }

            return new Page(page.Name, page.Path, document.DocumentNode.OuterHtml);
        }
    }
}
