namespace HtmlTemplater.Domain.Exceptions
{
    /// <summary>
    /// General parsing exception, indicates that a non-recoverable error occurred while parsing the specified page.
    /// </summary>
    public class ParsingException : Exception
    {
        public ParsingException(string page, int line, int column, string message) : base($"{page}:{line},{column}; {message}")
        {
        }

        public ParsingException(string page, int line, int column, string message, Exception? innerException) : base($"{page}:{line},{column}; {message}", innerException)
        {
        }
    }
}
