using HtmlAgilityPack;
using HtmlTemplater.Domain.Models;

namespace HtmlTemplater.Parsing.Agility.Interfaces
{
    public interface INodeParser
    {
        HtmlNode ParseNode(HtmlNode node, Element el, string documentName);
    }
}
