using HtmlAgilityPack;
using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Models;
using HtmlTemplater.Parsing.Agility.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace HtmlTemplater.Parsing.Agility.Services
{
    public class AgilityParser(ILogger<AgilityParser> _logger, IParseValidator _validator) : IParser
    {
        public int ElementCount { get => _elements.Count; } 

        private readonly SynchronizedCollection<Element> _elements = [];

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
            foreach (var el in _elements)
            {
                var nodes = document.DocumentNode.SelectNodes(el.Name.ToLower()) ?? new HtmlNodeCollection(document.DocumentNode);
                foreach(var node in nodes)
                {
                    try
                    {
                        var newNode = ParseNode(node, el, page);
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

        public HtmlNode ParseNode(HtmlNode node, Element el, Page page)
        {
            var attributes = GetAttributes(page, node);
            var placeholders = GetPlaceholders(el.Html);
            if (!placeholders.Any(p => p.Key == "innerhtml") && !string.IsNullOrEmpty(node.InnerHtml))
            {
                _validator.MissingInnerHtmlPlaceholder(page.Name, node.Line, node.LinePosition, el.Name);
            }

            var workNode = HtmlNode.CreateNode(el.Html);
            foreach (var item in placeholders)
            {
                switch (item.Key)
                {
                    // Special case is InnerHtml
                    case "innerhtml":
                        if ( string.IsNullOrEmpty(node.InnerHtml) )
                        {
                            _validator.MissingInnerHtml(page.Name, node.Line, node.LinePosition, el.Name);
                        }

                        workNode.InnerHtml = workNode.InnerHtml.Replace(item.Full, node.InnerHtml ?? string.Empty);
                        break;

                    // Default case is to use attributes on the outer node
                    default:
                        var att = attributes.Where(a => a.Key == item.Key);
                        if (!att.Any())
                        {
                            _validator.PlaceholderUnused(page.Name, node.Line, node.LinePosition, item.Full, el.Name);
                            workNode.InnerHtml = workNode.InnerHtml.Replace(item.Full, string.Empty);
                            break;
                        }

                        var attribute = att.First();
                        workNode.InnerHtml = workNode.InnerHtml.Replace(item.Full, attribute.Value);
                        attribute.Used = true;
                        break;
                }
            }

            foreach (var att in attributes.Where(a => !a.Used))
            {
                _validator.AttributeUnused(page.Name, node.Line, node.LinePosition, att.Key);
                if (!workNode.GetAttributes(att.Key).Any())
                {
                    workNode.Attributes.Add(att.Original); // Add original attribute back to the node
                }
            }

            return workNode;
        }

        public Element ParseElement(string name, string html)
        {
            if ( _elements.Any(e => e.Name == name) )
            {
                throw new Exception($"Attempting to parse element '{name}', but element was already parsed!");
            }

            var element = new Element(name, html);
            _elements.Add(element);
            return element;
        }
        
        private List<Attribute> GetAttributes(Page page, HtmlNode node)
        {
            var attributes = new List<Attribute>();
            foreach (var att in node.Attributes)
            {
                var newAtt = new Attribute(att);
                if (attributes.Any(a => a.Key == newAtt.Key))
                {
                    throw new DuplicateAttributeException(page.Name, att.Line, att.LinePosition, newAtt.Key);
                }
                attributes.Add(newAtt);
            }

            return attributes;
        }

        private List<Placeholder> GetPlaceholders(string html)
        {
            var result = new List<Placeholder>();
            int start = html.IndexOf("{{");
            while (start >= 0)
            {
                int end = html.IndexOf("}}", start) + 2;
                string key = html[(start + 2)..(end - 2)].ToLowerInvariant().Trim();
                string full = html[start..end];
                result.Add(new Placeholder(key,full));
                start = html.IndexOf("{{", end);
            }

            return result;
        }

        private record Placeholder(string Key, string Full);

        private class Attribute(HtmlAttribute Original)
        {
            public string Key { get; private set; } = Original.Name.ToLowerInvariant();
            public string Value { get; private set; } = Original.Value;
            public HtmlAttribute Original { get; private set; } = Original;
            public bool Used { get; set; } = false;
        }
    }
}
