using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Models;

namespace HtmlTemplater.Parsing.Agility.Services
{
    public class AgilityElementRepository : IElementRepository
    {
        public IReadOnlyCollection<string> KnownElements => [.. _elements.Select(e => e.Name)];

        private readonly SynchronizedCollection<Element> _elements = [];
        private bool _collectionResolved = false;

        /// <summary>
        /// Adds a new element definition to the <see cref="IElementRepository"/>
        /// </summary>
        /// <param name="name">Name of the element</param>
        /// <param name="html">HTML markup of the element</param>
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
            if (!_collectionResolved)
            {
                ResolveReferences();
            }

            var el = _elements.FirstOrDefault(e => e.Name == name);
            if ( el == null)
            {
                return null;
            }

            return new Element(el.Name, el.Html);
        }

        public Element GetRequired(string name) => Get(name) 
            ?? throw new Exception($"Element '{name}' does not exist in {typeof(AgilityElementRepository).FullName}");

        /// <summary>
        /// Resolves all element to element references
        /// </summary>
        private void ResolveReferences()
        {
        }
    }
}
