using HtmlAgilityPack;
using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Models;
using HtmlTemplater.Parsing.Agility.Interfaces;

namespace HtmlTemplater.Parsing.Agility.Services
{
    public class AgilityElementRepository(INodeParser _nodeParser) : IElementRepository
    {
        public IReadOnlyCollection<string> KnownElements => [.. _elements.Select(e => e.Name)];

        private readonly SynchronizedCollection<Element> _elements = [];
        private bool _collectionResolved = false;

        public void Add(string name, string html)
        {
            if (_elements.Any(e => e.Name == name))
            {
                throw new Exception($"Attempting to add element '{name}', but element was already present!");
            }

            var element = new Element(name, html);
            _elements.Add(element);
            _collectionResolved = false;
        }

        public Element? Get(string name)
        {
            EnsureReferencesResolved();
            return _elements.FirstOrDefault(e => e.Name == name);
        }

        public Element GetRequired(string name) => Get(name) 
            ?? throw new Exception($"Element '{name}' does not exist in {typeof(AgilityElementRepository).FullName}");

        private void EnsureReferencesResolved()
        {
            if (_collectionResolved)
            {
                return;
            }

            var replaced = new List<Tuple<Element, Element>>();
            foreach (var el in _elements)
            {
                var newEl = ResolveElementReferences(el, []);
                replaced.Add(new(el, newEl));
            }

            // Replace all elements that have been re-parsed with their new version
            replaced.ForEach(e =>
            {
                _elements.Remove(e.Item1);
                _elements.Add(e.Item2);
            });

            _collectionResolved = true;
        }

        private Element ResolveElementReferences(Element el, Element[] parents)
        {
            var outerElementDefinition = HtmlNode.CreateNode(el.Html);
            foreach (var elementDefinition in _elements)
            {
                var otherElements = outerElementDefinition.SelectNodes(elementDefinition.Name) ?? new HtmlNodeCollection(outerElementDefinition);
                if (otherElements.Count == 0)
                {
                    // Other element was not found in this element's definition
                    continue;
                }

                // Other element was found, ensure it's not also in the parent list somewhere, which indicates infinite recursion
                if ( parents.Any(p => p.Name == elementDefinition.Name) )
                {
                    throw new ParsingException(el.Name, otherElements[0].Line, otherElements[0].LinePosition, $"Infinite recursion detected for element {elementDefinition.Name}");
                }

                foreach (var item in otherElements)
                {
                    // Parse the node using the logic now in AgilityParser
                    var resultNode = _nodeParser.ParseNode(item, elementDefinition, el.Name);
                    item.ParentNode.ReplaceChild(resultNode, item);
                }
            }

            return new Element(el.Name, outerElementDefinition.OuterHtml);
        }
    }
}
