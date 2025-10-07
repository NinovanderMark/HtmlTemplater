using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Domain.Models;
using HtmlTemplater.Parsing.Agility.Interfaces;
using HtmlTemplater.Parsing.Agility.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HtmlTemplater.Parsing.Agility.Tests
{
    public class AgilityParserTests
    {
        [Fact]
        public void ReplaceInnerHtml_Happy()
        {
            // Assemble
            var validator = NSubstitute.Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string pageElement = "<div>{{InnerHtml}}</div>";
            var page = new Page("index", "index.html", "<page>Test</page>");
            var element = parser.ParseElement("page", pageElement);

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.Contains("Test", newPage.Content);
        }

        [Fact]
        public void ReplaceInnerHtml_NoInnerHtmlPlaceholder()
        {
            // Assemble
            var validator = NSubstitute.Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string pageElement = "<div>{{InnerHtmt}}</div>";
            var page = new Page("index", "index.html", "<page>Test</page>");
            var element = parser.ParseElement("page", pageElement);

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.DoesNotContain("Test", newPage.Content);

            validator.Received().MissingInnerHtmlPlaceholder(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
            validator.Received().PlaceholderUnused(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), "{{InnerHtmt}}", Arg.Any<string>());
        }

        [Fact]
        public void ReplaceInnerHtml_NoInnerHtml()
        {
            // Assemble
            var validator = NSubstitute.Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string pageElement = "<div>{{InnerHtml}}</div>";
            var page = new Page("index", "index.html", "<page></page>");
            var element = parser.ParseElement("page", pageElement);

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.DoesNotContain("{{InnerHtml}}", newPage.Content);

            validator.Received().MissingInnerHtml(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public void ReplaceInnerHtml_WithMultipleNodes_Exception()
        {
            // Assemble
            var validator = NSubstitute.Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string pageElement = "<header>Hello World</header><div>{{InnerHtml}}</div>";
            var page = new Page("index", "index.html", "<page>Test</page>");
            var element = parser.ParseElement("page", pageElement);

            // Act
            var result = Assert.Throws<ParsingException>(() => parser.ParsePage(page));

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Multiple node elements", result.Message);
        }

        [Fact]
        public void ReplaceInnerHtmlTitle_Happy()
        {
            // Assemble
            var validator = NSubstitute.Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string pageElement = "<body><header>{{Title}}</header><div>{{InnerHtml}}</div></body>";
            var page = new Page("index", "index.html", "<page title=\"Hello World\">Test</page>");
            var element = parser.ParseElement("page", pageElement);

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.Contains("<div>Test</div>", newPage.Content);
            Assert.Contains("<header>Hello World</header>", newPage.Content);
        }
    }
}
