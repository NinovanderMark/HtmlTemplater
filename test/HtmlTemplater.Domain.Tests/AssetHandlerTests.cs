using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HtmlTemplater.Domain.Tests
{
    public class AssetHandlerTests
    {
        [Fact]
        public void CopyAssetsDiscreet_Happy()
        {
            // Assemble
            var logger = Substitute.For<ILogger<AssetHandler>>();
            var filesystem = Substitute.For<IFileSystem>();
            var sut = new AssetHandler(logger, filesystem);
            var assets = new Dtos.AssetsDto();

            // Act
            sut.CopyAssetsDiscreet("root", "out", assets);

            // Assert
            filesystem.Received(1).CopyDirectory(Arg.Any<string>(), Arg.Any<string>(), true);
        }

        [Fact]
        public void CopyAssetsIntermixed_Happy()
        {
            // Assemble
            var logger = Substitute.For<ILogger<AssetHandler>>();
            var filesystem = Substitute.For<IFileSystem>();
            var sut = new AssetHandler(logger, filesystem);
            var assets = new Dtos.AssetsDto();

            // Act
            sut.CopyAssetsIntermixed("root/pages", "out", assets);

            // Assert
            filesystem.DidNotReceive().CopyDirectory(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
        }

        [Theory]
        [InlineData(".json", "input/manifest.json")]
        [InlineData("manifest*", "input/manifest1.json")]
        [InlineData("manifest.*", "input/manifest.json")]
        public void PathMatchesFilter_Matches(string filter, string path)
        {
            Assert.True(AssetHandler.PathMatchesFilter(path, filter));
        }

        [Theory]
        [InlineData(".jpg", "input/manifest.json")]
        [InlineData("*.json", "input/manifest.json.old")]
        public void PathMatchesFilter_DoesNotMatch(string path, string filter)
        {
            Assert.False(AssetHandler.PathMatchesFilter(path, filter));
        }
    }
}
