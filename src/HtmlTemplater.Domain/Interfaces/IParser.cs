using HtmlTemplater.Domain.Models;

namespace HtmlTemplater.Domain.Interfaces
{
    public interface IParser
    {
        Page ParsePage(Page page);
    }
}
