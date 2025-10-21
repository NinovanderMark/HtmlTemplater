using HtmlAgilityPack;
using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Domain.Models;
using HtmlTemplater.Parsing.Agility.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlTemplater.Parsing.Agility.Services
{
    public class AgilityNodeParser(IParseValidator _validator) : INodeParser
    {
        public HtmlNode ParseNode(HtmlNode node, Element el, string documentName)
        {
            var attributes = GetAttributes(documentName, node);
            var placeholders = GetPlaceholders(el.Html);
            if (!placeholders.Any(p => p.Key == "innerhtml") && !string.IsNullOrEmpty(node.InnerHtml))
            {
                _validator.MissingInnerHtmlPlaceholder(documentName, node.Line, node.LinePosition, el.Name);
            }

            var workNode = HtmlNode.CreateNode(el.Html);
            foreach (var item in placeholders)
            {
                switch (item.Key)
                {
                    // Special case is InnerHtml
                    case "innerhtml":
                        if (string.IsNullOrEmpty(node.InnerHtml))
                        {
                            _validator.MissingInnerHtml(documentName, node.Line, node.LinePosition, el.Name);
                        }

                        workNode.InnerHtml = workNode.InnerHtml.Replace(item.Full, node.InnerHtml ?? string.Empty);
                        break;

                    // Default case is to use attributes on the outer node
                    default:
                        var att = attributes.Where(a => a.Key == item.Key);
                        if (!att.Any())
                        {
                            _validator.PlaceholderUnused(documentName, node.Line, node.LinePosition, item.Full, el.Name);
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
                _validator.AttributeUnused(documentName, node.Line, node.LinePosition, att.Key);
                if (!workNode.GetAttributes(att.Key).Any())
                {
                    workNode.Attributes.Add(att.Original); // Add original attribute back to the node
                }
            }

            return workNode;
        }

        private List<Attribute> GetAttributes(string pageName, HtmlNode node)
        {
            var attributes = new List<Attribute>();
            foreach (var att in node.Attributes)
            {
                var newAtt = new Attribute(att);
                if (attributes.Any(a => a.Key == newAtt.Key))
                {
                    throw new DuplicateAttributeException(pageName, att.Line, att.LinePosition, newAtt.Key);
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
                result.Add(new Placeholder(key, full));
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
