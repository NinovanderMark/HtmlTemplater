using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Domain.Interfaces;
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
            var repository = Substitute.For<IElementRepository>();
            var parser = new AgilityParser(validator, repository);

            repository.KnownElements.Returns(["page"]);
            repository.GetRequired("page").Returns(new Element("page", "<div>{{InnerHtml}}</div>"));
            var page = new Page("index", "index.html", "<page>Test</page>");

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
            var repository = Substitute.For<IElementRepository>();
            var parser = new AgilityParser(validator, repository);

            repository.KnownElements.Returns(["page"]);
            repository.GetRequired("page").Returns(new Element("page", "<div>{{NotInnerHtml}}</div>"));
            var page = new Page("index", "index.html", "<page>Test</page>");

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.DoesNotContain("Test", newPage.Content);

            validator.Received().MissingInnerHtmlPlaceholder(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
            validator.Received().PlaceholderUnused(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), "{{NotInnerHtml}}", Arg.Any<string>());
        }

        [Fact]
        public void ParsePage_ReplaceInnerHtml_NoInnerHtmlWarning()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var repository = Substitute.For<IElementRepository>();
            var parser = new AgilityParser(validator, repository);

            repository.KnownElements.Returns(["page"]);
            repository.GetRequired("page").Returns(new Element("page", "<div>{{InnerHtml}}</div>"));
            var page = new Page("index", "index.html", "<page></page>");

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
            var repository = Substitute.For<IElementRepository>();
            var parser = new AgilityParser(validator, repository);

            repository.KnownElements.Returns(["page"]);
            repository.GetRequired("page").Returns(new Element("page", "<header>Hello World</header><div>{{InnerHtml}}</div>"));
            var page = new Page("index", "index.html", "<page>Test</page>");

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
            var repository = Substitute.For<IElementRepository>();
            var parser = new AgilityParser(validator, repository);

            repository.KnownElements.Returns(["page"]);
            repository.GetRequired("page").Returns(new Element("page", "<body><header>{{Title}}</header><div>{{InnerHtml}}</div></body>"));
            var page = new Page("index", "index.html", "<page title=\"Hello World\">Test</page>");

            // Act
            var newPage = parser.ParsePage(page);

            // Assert
            Assert.NotEqual(page, newPage);
            Assert.Contains("<div>Test</div>", newPage.Content);
            Assert.Contains("<header>Hello World</header>", newPage.Content);
        }
    }
}
