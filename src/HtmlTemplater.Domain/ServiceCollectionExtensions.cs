using HtmlTemplater.Domain.Interfaces;
using HtmlTemplater.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HtmlTemplater.Domain
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default implementation of <see cref="ISiteGenerator"/> to the service collection, along with its dependencies.
        /// </summary>
        public static IServiceCollection AddSiteGenerator(this IServiceCollection services)
        {
            services.AddScoped<ISiteGenerator, SiteGenerator>();
            services.AddTransient<IAssetHandler, AssetHandler>();
            services.AddTransient<IFileSystem, FileSystem>();
            return services;
        }
    }
}
