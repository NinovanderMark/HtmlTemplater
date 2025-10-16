using HtmlTemplater.Domain.Exceptions;
using HtmlTemplater.Parsing.Agility.Services;
using NSubstitute;
using Xunit;

namespace HtmlTemplater.Parsing.Agility.Tests
{
    public class AgilityElementRepositoryTests
    {
        [Theory]
        [InlineData("page", "<div>{{ InnerHtml }}</div>")]
        [InlineData("main", "<main>{{ InnerHtml }}</main>")]
        public void Get_Happy(string name, string html)
        {
            // Assemble
            var sut = new AgilityElementRepository();
            sut.Add(name, html);

            // Act
            var result = sut.Get(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(html, result.Html);
        }

        [Fact]
        public void Get_WithoutAdd_ReturnsNull()
        {
            // Assemble
            var sut = new AgilityElementRepository();

            // Act
            var result = sut.Get("page");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_NestedElements_Happy()
        {
            // Assemble
            var sut = new AgilityElementRepository();
            sut.Add("main", "<section>{{ InnerHtml }}</section>");
            sut.Add("page", "<body><main>{{ InnerHtml }}</main></body>");

            // Act
            var result = sut.Get("page");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("page", result.Name);
            Assert.Equal("<body><section>{{ InnerHtml }}</section></body>", result.Html);
        }

        [Fact]
        public void Get_NestedElements_ElementSelfReference_Happy()
        {
            // Assemble
            var sut = new AgilityElementRepository();
            sut.Add("main", "<div><main>{{ InnerHtml }}</main></div>");
            sut.Add("page", "<body><main>{{ InnerHtml }}</main></body>");

            // Act
            var result = sut.Get("page");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("page", result.Name);
            Assert.Equal("<body><div><main>{{ InnerHtml }}</main></div></body>", result.Html);
        }

        [Fact]
        public void Get_NestedElements_Recursion_ThrowsException()
        {
            // Assemble
            var sut = new AgilityElementRepository();
            sut.Add("main", "<div><page>{{ InnerHtml }}</page></div>");
            sut.Add("page", "<body><main>{{ InnerHtml }}</main></body>");

            // Act
            var result = Assert.Throws<ParsingException>(() => sut.Get("page"));

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Infinite recursion", result.Message);
        }
    }
}
