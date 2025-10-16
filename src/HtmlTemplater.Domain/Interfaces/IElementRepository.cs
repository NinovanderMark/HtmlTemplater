
using HtmlTemplater.Domain.Models;

namespace HtmlTemplater.Domain.Interfaces
{
    public interface IElementRepository
    {
        /// <summary>
        /// Returns all the elements that have been added to this <see cref="IElementRepository"/>.
        /// </summary>
        IReadOnlyCollection<string> KnownElements { get; }

        /// <summary>
        /// Adds a new element definition to the <see cref="IElementRepository"/>.
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <param name="html">HTML markup of the element.</param>
        void Add(string name, string html);

        /// <summary>
        /// Retrieve <see cref="Element"/> from the <see cref="IElementRepository"/> with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <returns>A new instance of <see cref="Element"/> for the given element <paramref name="name"/>, or <see cref="null"/> if no element with that name was added.</returns>
        Element? Get(string name);

        /// <summary>
        /// Retrieve <see cref="Element"/> from the <see cref="IElementRepository"/> with the given <paramref name="name"/>
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <returns>A new instance of <see cref="Element"/> for the given element <paramref name="name"/>, or throws an <see cref="Exception"/> if no element with that name was added.</returns>
        Element GetRequired(string name);
    }
}