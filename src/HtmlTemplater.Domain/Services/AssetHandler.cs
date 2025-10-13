using HtmlTemplater.Domain.Dtos;
using HtmlTemplater.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HtmlTemplater.Domain.Services
{
    public class AssetHandler(ILogger<AssetHandler> _logger, IFileSystem _fileSystem) : IAssetHandler
    {
        public void CopyAssetsDiscreet(string rootFolder, string outputPath, AssetsDto assets)
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

        public void CopyAssetsIntermixed(string pagesFolder, string outputPath, AssetsDto assets)
        {
            var assetFiles = _fileSystem.GetFiles(pagesFolder, "*.*", SearchOption.AllDirectories);
            var copyable = new List<string>();
            foreach (var assetFile in assetFiles)
            {
                bool included = false;
                if (assets.Include.Length == 0)
                {
                    // If there are no include filters, default to filtering out template files
                    included = Path.GetExtension(assetFile) != ".htmt";
                }

                // Include means that if the filepath contains one of the entries, it will be copied
                foreach (var inc in assets.Include)
                {
                    if (PathMatchesFilter(assetFile, inc))
                    {
                        included = true;
                    }
                }

                // Exclude means that if the filepath contains one of the entries, it will not be copied
                foreach (var ex in assets.Exclude)
                {
                    if (PathMatchesFilter(assetFile, ex))
                    {
                        included = false;
                    }
                }

                if (included)
                {
                    copyable.Add(assetFile);
                }
            }

            foreach (var file in copyable)
            {
                var relative = Path.GetRelativePath(file, pagesFolder);
                var outpath = Path.Combine(outputPath, relative);

                _logger.LogInformation("Copying asset file {AssetFile} to {OutputPath}", file, outpath);
                _fileSystem.CopyFile(file, outpath);
            }
        }

        public static bool PathMatchesFilter(string path, string filter)
        {
            if (filter == "*.*" || filter == ".")
            {
                return true;
            }

            string? startsWith = filter.EndsWith('*') ? filter[..^1] : null;
            string? endsWith = filter.StartsWith('*') ? filter[1..] : null;
            if (!string.IsNullOrEmpty(startsWith) && path.StartsWith(startsWith) ||
                !string.IsNullOrEmpty(endsWith) && path.EndsWith(endsWith) ||
                path.Contains(filter))
            {
                return true;
            }

            return false;
        }
    }
}
