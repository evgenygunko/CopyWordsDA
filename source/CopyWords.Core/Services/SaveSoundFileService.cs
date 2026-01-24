// Ignore Spelling: Downloader

using System.Runtime.Versioning;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface ISaveSoundFileService
    {
        Task<Stream> DownloadSoundFileAsync(string url, string word, CancellationToken cancellationToken);

        Task<bool> SaveSoundFileToAnkiFolderAsync(string url, string word, CancellationToken cancellationToken);
    }

    public class SaveSoundFileService : ISaveSoundFileService
    {
        private readonly IFileDownloaderService _fileDownloaderService;
        private readonly ISettingsService _settingsService;
        private readonly IFileIOService _fileIOService;
        private readonly IDialogService _dialogService;
        private readonly IGlobalSettings _globalSettings;

        public SaveSoundFileService(
            IFileDownloaderService fileDownloaderService,
            ISettingsService settingsService,
            IFileIOService fileIOService,
            IDialogService dialogService,
            IGlobalSettings globalSettings)
        {
            _fileDownloaderService = fileDownloaderService;
            _settingsService = settingsService;
            _fileIOService = fileIOService;
            _dialogService = dialogService;
            _globalSettings = globalSettings;
        }

        public async Task<Stream> DownloadSoundFileAsync(string url, string word, CancellationToken cancellationToken)
        {
            string downloadSoundUrl = CreateDownloadSoundFileUrl(url, word);
            return await _fileDownloaderService.DownloadFileAsync(downloadSoundUrl, cancellationToken);
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        public async Task<bool> SaveSoundFileToAnkiFolderAsync(string url, string word, CancellationToken cancellationToken)
        {
            // On Windows and Mac, download the file and save it directly to the Anki collection media folder.
            string ankiSoundsFolder = _settingsService.LoadSettings().AnkiSoundsFolder;
            if (!_fileIOService.DirectoryExists(ankiSoundsFolder))
            {
                await _dialogService.DisplayAlertAsync("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{ankiSoundsFolder}'. Please update it in Settings.", "OK");
                return false;
            }

            // We download sound file via our backend, which will transcode it to mp3 format.
            string downloadSoundUrl = CreateDownloadSoundFileUrl(url, word);
            using Stream stream = await _fileDownloaderService.DownloadFileAsync(downloadSoundUrl, cancellationToken);

            string fileName = $"{word}.mp3";
            string destinationFile = Path.Combine(ankiSoundsFolder, fileName);
            if (_fileIOService.FileExists(destinationFile))
            {
                bool answer = await _dialogService.DisplayAlertAsync("File already exists", $"File '{fileName}' already exists. Overwrite?", "Yes", "No");
                if (!answer)
                {
                    // User doesn't want to overwrite the file, so we can skip the download. But the file already exists, so we return true.
                    return true;
                }
            }

            await _fileIOService.CopyToAsync(stream, destinationFile, cancellationToken);
            return true;
        }

        internal string CreateDownloadSoundFileUrl(string soundUrl, string word)
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v1/Sound/DownloadSound?" +
                   $"soundUrl={Uri.EscapeDataString(soundUrl)}&word={Uri.EscapeDataString(word)}&version=1&code={_globalSettings.TranslatorAppRequestCode}";
        }
    }
}
