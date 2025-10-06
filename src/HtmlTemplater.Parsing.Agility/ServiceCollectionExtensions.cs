using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Parsing.Agility.Interfaces;
using HtmlTemplater.Parsing.Agility.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HtmlTemplater.Parsing.Agility
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the AgilityParser implementation of IParser to the service collection, along with its dependencies.
        /// </summary>
        public static IServiceCollection AddAgilityParser(this IServiceCollection services)
        {
            services.AddTransient<IParser, AgilityParser>();
            services.AddTransient<IParseValidator, ParseValidator>();
            return services;
        }
    }
}
