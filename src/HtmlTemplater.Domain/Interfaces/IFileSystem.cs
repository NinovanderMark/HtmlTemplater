using System.Text.Json;

namespace HtmlTemplater.Domain.Interfaces
{
    public interface IFileSystem
    {
        void CopyDirectory(string sourceDir, string destinationDir, bool recursive);
        void CopyFile(string file, string outpath);
        void DeleteFileIfExists(string path);
        void EnsureDirectoryExists(string path);
        bool FileExists(string path);
        string GetDirectoryName(string path);
        string[] GetFiles(string path, string searchPattern, SearchOption searchOptions);
        Task<string> ReadAllTextAsync(string path);
        Task<T> ReadAndDeserializeAsync<T>(string path, JsonSerializerOptions? options = null) where T : notnull;
        Task WriteAllTextAsync(string path, string content, CancellationToken token = default);
    }
}
