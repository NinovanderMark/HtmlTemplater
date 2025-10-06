namespace HtmlTemplater.Domain.Exceptions
{
    /// <summary>
    /// Exception indicating that an attribute occurred more than once for an element.
    /// </summary>
    public class DuplicateAttributeException : ParsingException
    {
        public DuplicateAttributeException(string page, int line, int column, string attribute) : base(page, line, column, $"Attribute '{attribute}' occurred more than once for element")
        {
        }

        public DuplicateAttributeException(string page, int line, int column, string attribute, Exception? innerException) : base(page, line, column, $"Attribute '{attribute}' occurred more than once for element", innerException)
        {
        }
    }
}
