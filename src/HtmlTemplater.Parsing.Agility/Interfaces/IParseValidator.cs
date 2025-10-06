namespace HtmlTemplater.Parsing.Agility.Interfaces
{
    public interface IParseValidator
    {
        void AttributeUnused(string pageName, int line, int linePosition, string attribute);
        void MissingInnerHtml(string pageName, int line, int linePosition, string elementName);
        void MissingInnerHtmlPlaceholder(string pageName, int line, int linePosition, string elementName);
        void PlaceholderUnused(string pageName, int line, int linePosition, string placeholder, string elementName);
    }
}
