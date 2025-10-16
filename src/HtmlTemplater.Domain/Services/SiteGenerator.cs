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
    public class SiteGenerator(
        ILogger<SiteGenerator> _logger, 
        IFileSystem _fileSystem,
        IAssetHandler _assetHandler,
        IParser _parser,
        IElementRepository _repository) : ISiteGenerator
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
            _logger.LogInformation("Parsing assets before copying them to {OutputFolder}", outputPath);

            var pagesFolder = Path.Combine(rootFolder, Pages);
            if (manifest.Assets?.Input == null)
            {
                _assetHandler.CopyAssetsIntermixed(pagesFolder, outputPath, manifest.Assets ?? new());
            }
            else
            {
                _assetHandler.CopyAssetsDiscreet(rootFolder, outputPath, manifest.Assets);
            }

            await ParseElements(rootFolder, manifest);
            await ParsePages(pagesFolder, outputPath);

            _logger.LogInformation("Operation completed successfully");
            return 0;
        }

        private async Task ParseElements(string rootFolder, ManifestDto manifest)
        {
            var elementFolder = Path.Combine(rootFolder, Elements);

            _logger.LogInformation("Reading elements from {ElementFolder}", elementFolder);
            foreach (var e in manifest.Elements ?? [])
            {
                var elementFile = Path.Combine(elementFolder, $"{e}.htmt");
                var content = await _fileSystem.ReadAllTextAsync(elementFile);
                _repository.Add(e, content);
            }
        }

        private async Task ParsePages(string pagesFolder, string outputPath)
        {
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

            _logger.LogInformation("Parsing {PageCount} pages using {ElementCount} known elements", pages.Count, _repository.KnownElements.Count);
            foreach (var page in pages)
            {
                parsedPages.Add(_parser.ParsePage(page));
            }

            _logger.LogInformation("Writing parsed pages to {OutputPath}", outputPath);
            foreach (var page in parsedPages)
            {
                var relativePath = Path.GetRelativePath(pagesFolder, page.Path);
                var pageOutputPath = Path.ChangeExtension(Path.Combine(outputPath, relativePath), ".html");

                string? parentDirectory = Path.GetDirectoryName(pageOutputPath);
                if (parentDirectory != null)
                {
                    _fileSystem.EnsureDirectoryExists(parentDirectory);
                }

                _logger.LogInformation("Writing {PagePath}", pageOutputPath);
                _fileSystem.DeleteFileIfExists(pageOutputPath);
                await _fileSystem.WriteAllTextAsync(pageOutputPath, page.Content);
            }            
        }
    }
}
