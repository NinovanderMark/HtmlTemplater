using HtmlTemplater.Parsing.Agility.Interfaces;
using Microsoft.Extensions.Logging;

namespace HtmlTemplater.Parsing.Agility.Services
{
    public class ParseValidator(ILogger<ParseValidator> _logger) : IParseValidator
    {
        public void AttributeUnused(string pageName, int line, int linePosition, string attribute)
        {
            _logger.LogWarning("{PageName}:{Line},{Column}; Attribute '{Attribute}' specified but unused",
                pageName, line, linePosition, attribute);
        }

        public void MissingInnerHtml(string pageName, int line, int linePosition, string elementName)
        {
            _logger.LogWarning("{PageName}:{Line},{Column}; No inner HTML provided for element '{ElementName}'",
                pageName, line, linePosition, elementName);
        }

        public void MissingInnerHtmlPlaceholder(string pageName, int line, int linePosition, string elementName)
        {
            _logger.LogWarning("{PageName}:{Line},{Column}; Inner HTML provided for element '{ElementName}' without replaceable token",
                pageName, line, linePosition, elementName);
        }

        public void PlaceholderUnused(string pageName, int line, int linePosition, string placeholder, string elementName)
        {
            _logger.LogWarning("{PageName}:{Line},{Column}; No attribute specified for placeholder '{Placeholder}' in element '{Element}'",
                pageName, line, linePosition, placeholder, elementName);
        }
    }
}
