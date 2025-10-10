using HtmlTemplater.Domain.Dtos;
using HtmlTemplater.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HtmlTemplater.Domain.Services
{
    public class FileSystem(ILogger<FileSystem> _logger) : IFileSystem
    {
        public void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
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
                file.CopyTo(targetFilePath, overwrite: true);
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

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public async Task<T> ReadAndDeserializeAsync<T>(string path, JsonSerializerOptions? options = null) where T : notnull
        {
            return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path), options)
                ?? throw new SerializationException($"Deserializing file at '{path}' as '{typeof(T).FullName}' was unsuccessful");
        }

        public void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogInformation("Creating new directory {Path}", path);
                Directory.CreateDirectory(path);
            }
        }

        public void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public async Task<string> ReadAllTextAsync(string path)
        {
            return await File.ReadAllTextAsync(path);
        }

        public string[] GetFiles(string path, string searchPattern, SearchOption searchOptions)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public async Task WriteAllTextAsync(string path, string content, CancellationToken token = new())
        {
            await File.WriteAllTextAsync(path, content, token);
        }

        public string GetDirectoryName(string path)
        {
            return new FileInfo(path).Directory?.FullName ?? throw new Exception($"Unable to retrieve path information for {path}");
        }
    }
}
