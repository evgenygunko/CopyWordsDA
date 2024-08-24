namespace CopyWords.Core.Services
{
    public interface IFileIOService
    {
        bool DirectoryExists(string path);

        bool FileExists(string path);
    }

    public class FileIOService : IFileIOService
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);

        public bool FileExists(string path) => File.Exists(path);
    }
}
