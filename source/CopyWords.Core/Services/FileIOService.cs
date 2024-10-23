namespace CopyWords.Core.Services
{
    public interface IFileIOService
    {
        bool DirectoryExists(string path);

        bool FileExists(string path);

        Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

        Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);
    }

    public class FileIOService : IFileIOService
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);

        public bool FileExists(string path) => File.Exists(path);

        public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) => await File.ReadAllTextAsync(path);

        public async Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default) => await File.WriteAllTextAsync(path, contents);
    }
}
