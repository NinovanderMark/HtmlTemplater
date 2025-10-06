using HtmlTemplater.Domain.Models;

namespace HtmlTemplater.Domain.Interfaces
{
    public interface IParser
    {
        int ElementCount { get; }

        Page ParsePage(Page page);
        Element ParseElement(string name, string html);
    }
}
