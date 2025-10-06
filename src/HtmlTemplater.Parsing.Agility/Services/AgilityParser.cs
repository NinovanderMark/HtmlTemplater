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
                        ParseNode(node, page, el);
                    }
                    catch (Exception ex)
                    {
                        throw new ParsingException(page.Name, node.Line, node.LinePosition, ex.Message, ex);
                    }
                }
            }

            return new Page(page.Name, page.Path, document.DocumentNode.OuterHtml);
        }

        public void ParseNode(HtmlNode node, Page page, Element el)
        {
            var attributes = GetAttributes(page, node);
            var placeholders = GetPlaceholders(el.Html);
            if (!placeholders.Any(p => p.Key == "innerhtml") && !string.IsNullOrEmpty(node.InnerHtml))
            {
                _validator.MissingInnerHtmlPlaceholder(page.Name, node.Line, node.LinePosition, el.Name);
            }

            var workNode = node;
            foreach (var item in placeholders)
            {
                switch (item.Key)
                {
                    // Special case is InnerHtml
                    case "innerhtml":
                        workNode = ReplaceInnerHtml(item.Full, workNode, el, page);
                        break;

                    // Default case is to use attributes on the outer node
                    default:
                        var att = attributes.Where(a => a.Key == item.Key);
                        if (!att.Any())
                        {
                            _validator.PlaceholderUnused(page.Name, node.Line, node.LinePosition, item.Full, el.Name);
                            workNode = ReplaceWithValue(item, string.Empty, workNode, el, page);
                            break;
                        }

                        var attribute = att.First();
                        workNode = ReplaceWithValue(item, attribute.Value, workNode, el, page);
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
            }

            return attributes;
        }

        private HtmlNode ReplaceWithValue(Placeholder replaceable, string value, HtmlNode node, Element el, Page page)
        {
            var newNode = HtmlNode.CreateNode(el.Html.Replace(replaceable.Full, value));
            node.ParentNode.ReplaceChild(newNode, node);
            return newNode;
        }

        private HtmlNode ReplaceInnerHtml(string placeholder, HtmlNode node, Element el, Page page)
        {
            var innerHtml = node.InnerHtml;
            if (!string.IsNullOrEmpty(innerHtml))
            {
                // Replace element placeholder, and use its inner HTML instead of the definition's {{ InnerHtml }} 
                var newNode = HtmlNode.CreateNode(el.Html.Replace(placeholder, innerHtml));
                node.ParentNode.ReplaceChild(newNode, node);
                return newNode;
            }

            _validator.MissingInnerHtml(page.Name, node.Line, node.LinePosition, el.Name);

            // Replace element placeholder, removing the {{ InnerHtml }} token
            var newNode2 = HtmlNode.CreateNode(el.Html.Replace(placeholder, string.Empty));
            node.ParentNode.ReplaceChild(newNode2, node);
            return newNode2;
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
