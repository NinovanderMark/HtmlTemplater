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
        public void ParsePage_ReplaceInnerHtml_Happy()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
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
        public void ParsePage_ReplaceInnerHtml_NoInnerHtmlPlaceholder()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
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
        public void ParsePage_ReplaceInnerHtml_NoInnerHtmlWarning()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
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
        public void ParsePage_ReplaceInnerHtml_WithMultipleNodes_Exception()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
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
        public void ParsePage_ReplaceInnerHtmlTitle_Happy()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
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

        [Fact]
        public void ParsePage_NestedElements_Happy()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string mainElementHtml = "<section>{{InnerHtml}}</section>";
            string pageElementHtml = "<body><main>{{InnerHtml}}</main></body>";
            var mainElement = parser.ParseElement("main", mainElementHtml);
            var pageElement = parser.ParseElement("page", pageElementHtml);
            var page = new Page("index", "index.html", "<page>Hello World</page>");

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.Contains("<body><section>Hello World</section></body>", newPage.Content);
        }

        [Fact]
        public void ParsePage_NestedElements_ElementSelfReference_Happy()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string mainElementHtml = "<div><main>{{InnerHtml}}</main></div>";
            string pageElementHtml = "<body><main>{{InnerHtml}}</main></body>";
            var mainElement = parser.ParseElement("main", mainElementHtml);
            var pageElement = parser.ParseElement("page", pageElementHtml);
            var page = new Page("index", "index.html", "<page>Hello World</page>");

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.Contains("<body><div><main>Hello World</main></div></body>", newPage.Content);
        }

        [Fact]
        public void ParsePage_NestedElements_Recursion_ThrowsException()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityParser(validator);

            string mainElementHtml = "<div><page>{{InnerHtml}}</page></div>";
            string pageElementHtml = "<body><main>{{InnerHtml}}</main></body>";
            var mainElement = parser.ParseElement("main", mainElementHtml);
            var pageElement = parser.ParseElement("page", pageElementHtml);
            var page = new Page("index", "index.html", "<page>Hello World</page>");

            // Act
            var result = Assert.Throws<ParsingException>(() => parser.ParsePage(page));

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Infinite recursion", result.Message);
        }
    }
}
