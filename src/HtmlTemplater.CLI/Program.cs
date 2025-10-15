using HtmlTemplater.Domain;
using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Models;
using HtmlTemplater.Domain.Services;
using HtmlTemplater.Parsing.Agility;
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
                    Console.WriteLine($"v{Version}");
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
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Starting {Application} v{Version}", Name, Version);

                var generator = serviceProvider.GetRequiredService<ISiteGenerator>();
                return await generator.GenerateFromManifest(manifestPath);
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
                .AddSiteGenerator()
                .AddAgilityParser()
                .BuildServiceProvider();
    }
}
