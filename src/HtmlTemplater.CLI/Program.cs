using HtmlTemplater.Domain;
using HtmlTemplater.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace HtmlTemplater.CLI
{
    internal class Program
    {
        internal static string Name = "HtmlTemplater";
        internal static string? Version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        static async Task<int> Main(string[] args)
        {
            string manifestPath = Path.Combine(Environment.CurrentDirectory, "manifest.json");
            if (args.Length > 0)
            {
                if (args[0] == "-v" || args[0] == "--version")
                {
                    Console.WriteLine($"{Name} v{Version}");
                    return 0;
                }

                manifestPath = Path.Combine(Environment.CurrentDirectory, string.Concat(args));
            }

            IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
              .AddEnvironmentVariables()
              .Build();

            var serviceProvider = ConfigureServices(configuration);

            try
            {
                return await Run(serviceProvider, manifestPath);
            }
            catch (Exception ex)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = color;
                return -1;
            }
            finally
            {
                serviceProvider.Dispose();
            }

        }

        private static ServiceProvider ConfigureServices(IConfiguration configuration)
            => new ServiceCollection()
                .AddSingleton(configuration)
                .AddLogging(configure => configure.AddSimpleConsole(o =>
                {
                    o.IncludeScopes = true;
                    o.SingleLine = true;
                    o.TimestampFormat = "[HH:mm:ss] ";
                }))
                .AddTransient<AgilityParser>()
                .BuildServiceProvider();

        private static async Task<int> Run(IServiceProvider services, string manifestPath)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting {Application} v{Version}", Name, Version);

            if ( !File.Exists(manifestPath) )
            {
                logger.LogError("Unable to find manifest file at '{ManifestPath}'", manifestPath);
                return 2;
            }
            var serializerOptions = new JsonSerializerOptions()
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = SourceGenerationContext.Default
            };

            logger.LogInformation("Reading Manifest file from {ManifestPath}", manifestPath);
            var manifest = JsonSerializer.Deserialize<ManifestDto>(await File.ReadAllTextAsync(manifestPath), serializerOptions)
                ?? throw new Exception("Deserializing manifest was unsuccessful");

            string rootFolder = new FileInfo(manifestPath).Directory?.FullName 
                ?? throw new Exception($"Unable to retrieve path information for {manifestPath}");

            string outputPath = Path.Combine(rootFolder, "out");
            if ( !string.IsNullOrWhiteSpace(manifest.OutputPath) )
            {
                outputPath = Path.Combine(rootFolder, manifest.OutputPath);
            }

            if ( !Directory.Exists(outputPath) )
            {
                logger.LogInformation("Creating output directory at {OutputPath}", outputPath);
                Directory.CreateDirectory(outputPath);
            }

            string assetsSource = Path.Combine(rootFolder, "assets");
            string assetsDestination = Path.Combine(outputPath, "assets");
            if (!string.IsNullOrWhiteSpace(manifest.AssetFolder))
            {
                assetsSource = Path.Combine(rootFolder, manifest.AssetFolder);
                assetsDestination = Path.Combine(outputPath, manifest.AssetFolder);
            }

            logger.LogInformation("Copying assets to {AssetFolder}", assetsDestination);
            CopyDirectory(assetsSource, assetsDestination, recursive: true);

            var elements = new List<Element>();
            var elementFolder = Path.Combine(rootFolder, "elements");

            logger.LogInformation("Reading elements from {ElementFolder}", elementFolder);
            foreach (var e in manifest.Elements ?? [])
            {
                var elementFile = Path.Combine(elementFolder, $"{e}.htmt");
                var content = await File.ReadAllTextAsync(elementFile);
                elements.Add(new Element(e, content));
            }

            var pagesFolder = Path.Combine(rootFolder, "pages");
            logger.LogInformation("Discovering pages from {PagesFolder}", pagesFolder);
            var pageFiles = Directory.GetFiles(pagesFolder, "*.htmt", SearchOption.AllDirectories);
            var pages = new List<Page>();

            logger.LogInformation("Reading content from {PageCount} pages", pageFiles.Length);
            foreach (var file in pageFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var page = new Page(Path.GetFileName(file), Path.GetFullPath(file), content);
                pages.Add(page);
            }

            var parser = services.GetRequiredService<AgilityParser>();
            parser.AddElements(elements);
            var parseTasks = new List<Task<Page>>();

            logger.LogInformation("Parsing {PageCount} pages using {ElementCount} known elements", pages.Count, elements.Count);
            foreach (var page in pages)
            {
                parseTasks.Add(parser.ParsePage(page));
            }

            await Task.WhenAll(parseTasks);

            logger.LogInformation("Writing parsed pages to {OutputPath}", outputPath);
            foreach (var task in parseTasks)
            {
                var page = await task;
                var relativePath = Path.GetRelativePath(pagesFolder, page.Path);
                var pageOutputPath = Path.ChangeExtension(Path.Combine(outputPath, relativePath), ".html");
                await File.WriteAllTextAsync(pageOutputPath, page.Content);
            }

            logger.LogInformation("Operation completed");

            return 0;
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
