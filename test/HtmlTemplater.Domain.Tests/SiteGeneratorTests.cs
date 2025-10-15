using HtmlTemplater.Domain.Dtos;
using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace HtmlTemplater.Domain.Tests
{
    public class SiteGeneratorTests
    {
        [Fact]
        public async Task GenerateFromManifest_Happy()
        {
            string path = "manifest.json";

            // Assemble
            var logger = Substitute.For<ILogger<SiteGenerator>>();
            var filesystem = Substitute.For<IFileSystem>();
            var assethandler = Substitute.For<IAssetHandler>();
            var parser = Substitute.For<IParser>();
            var sut = new SiteGenerator(logger, filesystem, assethandler, parser);

            var manifestDto = new ManifestDto();

            filesystem.FileExists(path).Returns(true);
            filesystem.ReadAndDeserializeAsync<ManifestDto>(path, Arg.Any<JsonSerializerOptions>()).Returns(manifestDto);

            // Act
            int result = await sut.GenerateFromManifest(path);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GenerateFromManifest_DiscreetMode()
        {
            string path = "manifest.json";

            // Assemble
            var logger = Substitute.For<ILogger<SiteGenerator>>();
            var filesystem = Substitute.For<IFileSystem>();
            var assethandler = Substitute.For<IAssetHandler>();
            var parser = Substitute.For<IParser>();
            var sut = new SiteGenerator(logger, filesystem, assethandler, parser);

            var manifestDto = new ManifestDto() { Assets = new() { Input = "assets" } };

            filesystem.FileExists(path).Returns(true);
            filesystem.ReadAndDeserializeAsync<ManifestDto>(path, Arg.Any<JsonSerializerOptions>()).Returns(manifestDto);

            // Act
            int result = await sut.GenerateFromManifest(path);

            // Assert
            Assert.Equal(0, result);
            assethandler.Received(1).CopyAssetsDiscreet(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AssetsDto>());
            assethandler.DidNotReceive().CopyAssetsIntermixed(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AssetsDto>());
        }

        [Fact]
        public async Task GenerateFromManifest_IntermixedMode()
        {
            string path = "manifest.json";

            // Assemble
            var logger = Substitute.For<ILogger<SiteGenerator>>();
            var filesystem = Substitute.For<IFileSystem>();
            var assethandler = Substitute.For<IAssetHandler>();
            var parser = Substitute.For<IParser>();
            var sut = new SiteGenerator(logger, filesystem, assethandler, parser);

            var manifestDto = new ManifestDto();

            filesystem.FileExists(path).Returns(true);
            filesystem.ReadAndDeserializeAsync<ManifestDto>(path, Arg.Any<JsonSerializerOptions>()).Returns(manifestDto);

            // Act
            int result = await sut.GenerateFromManifest(path);

            // Assert
            Assert.Equal(0, result);
            assethandler.DidNotReceive().CopyAssetsDiscreet(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AssetsDto>());
            assethandler.Received(1).CopyAssetsIntermixed(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AssetsDto>());
        }
    }
}
