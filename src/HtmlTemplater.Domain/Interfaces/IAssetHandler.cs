using HtmlTemplater.Domain.Dtos;

namespace HtmlTemplater.Domain.Interfaces
{
    public interface IAssetHandler
    {
        void CopyAssetsDiscreet(string rootFolder, string outputPath, AssetsDto assets);
        void CopyAssetsIntermixed(string pagesFolder, string outputPath, AssetsDto assets);
    }
}
