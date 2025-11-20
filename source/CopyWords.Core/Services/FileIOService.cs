// Ignore Spelling: dest

namespace CopyWords.Core.Services
{
    public interface IFileIOService
    {
        bool DirectoryExists(string? path);

        bool FileExists(string? path);

        void CopyFile(string sourceFileName, string destFileName, bool overwrite);

        Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

        Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default);

        Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

        Task CopyToAsync(Stream sourceStream, string destinationFile, CancellationToken cancellationToken = default);
    }

    public class FileIOService : IFileIOService
    {
        public bool DirectoryExists(string? path)
            => Directory.Exists(path);

        public bool FileExists(string? path)
            => File.Exists(path);

        public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
            => File.Copy(sourceFileName, destFileName, overwrite);

        public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
            => await File.ReadAllTextAsync(path);

        public async Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
            => await File.WriteAllTextAsync(path, contents);

        public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
            => await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        public async Task CopyToAsync(Stream sourceStream, string destinationFile, CancellationToken cancellationToken = default)
        {
            using var fileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(fileStream, cancellationToken);
        }
    }
}
