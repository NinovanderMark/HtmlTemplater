using HtmlAgilityPack;
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
    public class AgilityNodeParserTests
    {
        [Fact]
        public void ParsePage_ReplaceInnerHtml_Happy()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityNodeParser(validator);

            var node = HtmlNode.CreateNode("<page>Test</page>");
            var element = new Element("page", "<div>{{InnerHtml}}</div>");

            // Act
            var newNode = parser.ParseNode(node, element, string.Empty);

            // Assert
            Assert.NotEqual(node, newNode);
            Assert.Contains("Test", newNode.OuterHtml);
        }

        [Fact]
        public void ParsePage_ReplaceInnerHtml_NoInnerHtmlPlaceholder()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityNodeParser(validator);

            var node = HtmlNode.CreateNode("<page>Test</page>");
            var element = new Element("page", "<div>{{NotInnerHtml}}</div>");

            // Act
            var newNode = parser.ParseNode(node, element, string.Empty);

            // Assert
            Assert.NotEqual(node, newNode);
            Assert.DoesNotContain("Test", newNode.OuterHtml);

            validator.Received().MissingInnerHtmlPlaceholder(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
            validator.Received().PlaceholderUnused(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), "{{NotInnerHtml}}", Arg.Any<string>());
        }

        [Fact]
        public void ParsePage_ReplaceInnerHtml_NoInnerHtmlWarning()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityNodeParser(validator);

            var node = HtmlNode.CreateNode("<page></page>");
            var element = new Element("page", "<div>{{InnerHtml}}</div>");

            // Act
            var newNode = parser.ParseNode(node, element, string.Empty);

            // Assert
            Assert.NotEqual(node, newNode);
            Assert.DoesNotContain("{{InnerHtml}}", newNode.OuterHtml);

            validator.Received().MissingInnerHtml(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public void ParsePage_ReplaceInnerHtml_WithMultipleNodes_Exception()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityNodeParser(validator);

            var node = HtmlNode.CreateNode("<page></page>");
            var element = new Element("page", "<header>Hello World</header><div>{{InnerHtml}}</div>");

            // Act
            var result = Assert.Throws<Exception>(() => parser.ParseNode(node, element, string.Empty));

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Multiple node elements", result.Message);
        }

        [Fact]
        public void ParsePage_ReplaceInnerHtmlTitle_Happy()
        {
            // Assemble
            var validator = Substitute.For<IParseValidator>();
            var parser = new AgilityNodeParser(validator);

            var node = HtmlNode.CreateNode("<page title=\"Hello World\">Test</page>");
            var element = new Element("page", "<body><header>{{Title}}</header><div>{{InnerHtml}}</div></body>");

            // Act
            var newNode = parser.ParseNode(node, element, string.Empty);

            // Assert
            Assert.NotEqual(node, newNode);
            Assert.Contains("<div>Test</div>", newNode.OuterHtml);
            Assert.Contains("<header>Hello World</header>", newNode.OuterHtml);
        }
    }
}
