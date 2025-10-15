namespace HtmlTemplater.Domain.Interfaces
{
    public interface ISiteGenerator
    {
        Task<int> GenerateFromManifest(string manifestPath);
    }
}
