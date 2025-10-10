using HtmlTemplater.Domain.Dtos;
using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HtmlTemplater.Domain.Services
{
    public class SiteGenerator(ILogger<SiteGenerator> _logger, IFileSystem _fileSystem, IParser _parser) : ISiteGenerator
    {
        public static readonly string Elements = "elements";
        public static readonly string Pages = "pages";

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = SourceGenerationContext.Default
        };

        public async Task<int> GenerateFromManifest(string manifestPath)
        {
            if (!_fileSystem.FileExists(manifestPath))
            {
                _logger.LogError("Unable to find manifest file at '{ManifestPath}'", manifestPath);
                return 2;
            }

            _logger.LogInformation("Reading Manifest file from {ManifestPath}", manifestPath);
            var manifest = await _fileSystem.ReadAndDeserializeAsync<ManifestDto>(manifestPath, _jsonSerializerOptions);

            string rootFolder = _fileSystem.GetDirectoryName(manifestPath);
            string outputPath = Path.Combine(rootFolder, "out");
            if (!string.IsNullOrWhiteSpace(manifest.OutputPath))
            {
                outputPath = Path.Combine(rootFolder, manifest.OutputPath);
            }

            _fileSystem.EnsureDirectoryExists(outputPath);

            if (manifest.Assets?.Input == null)
            {
                CopyAssetsIntermixed(rootFolder, outputPath);
            }
            else
            {
                CopyAssetsDiscreet(rootFolder, outputPath, manifest.Assets);
            }

            var elementFolder = Path.Combine(rootFolder, Elements);

            _logger.LogInformation("Reading elements from {ElementFolder}", elementFolder);
            foreach (var e in manifest.Elements ?? [])
            {
                var elementFile = Path.Combine(elementFolder, $"{e}.htmt");
                var content = await _fileSystem.ReadAllTextAsync(elementFile);
                _parser.ParseElement(e, content);
            }

            var pagesFolder = Path.Combine(rootFolder, Pages);
            _logger.LogInformation("Discovering pages from {PagesFolder}", pagesFolder);
            var pageFiles = _fileSystem.GetFiles(pagesFolder, "*.htmt", SearchOption.AllDirectories);
            var pages = new List<Page>();

            _logger.LogInformation("Reading content from {PageCount} pages", pageFiles.Length);
            foreach (var file in pageFiles)
            {
                var content = await _fileSystem.ReadAllTextAsync(file);
                var page = new Page(Path.GetFileName(file), Path.GetFullPath(file), content);
                pages.Add(page);
            }

            var parsedPages = new List<Page>();

            _logger.LogInformation("Parsing {PageCount} pages using {ElementCount} known elements", pages.Count, _parser.ElementCount);
            foreach (var page in pages)
            {
                parsedPages.Add(_parser.ParsePage(page));
            }

            _logger.LogInformation("Writing parsed pages to {OutputPath}", outputPath);
            foreach (var page in parsedPages)
            {
                var relativePath = Path.GetRelativePath(pagesFolder, page.Path);
                var pageOutputPath = Path.ChangeExtension(Path.Combine(outputPath, relativePath), ".html");
                _fileSystem.DeleteFileIfExists(pageOutputPath);

                _logger.LogInformation("Writing {PagePath}", pageOutputPath);
                await _fileSystem.WriteAllTextAsync(pageOutputPath, page.Content);
            }

            _logger.LogInformation("Operation completed");

            return 0;
        }

        private void CopyAssetsDiscreet(string rootFolder, string outputPath, AssetsDto assets)
        {
            string assetsSource = Path.Combine(rootFolder, "assets");
            if (!string.IsNullOrWhiteSpace(assets.Input))
            {
                assetsSource = Path.Combine(rootFolder, assets.Input);
            }

            string assetsDestination = Path.Combine(outputPath, "assets");
            if (assets.Output != null)
            {
                assetsDestination = Path.Combine(outputPath, assets.Output);
            }

            _logger.LogInformation("Copying all assets to {AssetFolder}", assetsDestination);
            _fileSystem.CopyDirectory(assetsSource, assetsDestination, recursive: true);
        }

        private void CopyAssetsIntermixed(string rootFolder, string outputPath)
        {
            throw new NotImplementedException("Intermixed mode not yet implemented, aborting");
        }
    }
}
