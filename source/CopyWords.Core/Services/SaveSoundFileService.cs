// Ignore Spelling: Downloader

using System.Runtime.Versioning;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface ISaveSoundFileService
    {
        Task<bool> SaveSoundFileAsync(string url, string word, CancellationToken cancellationToken);
    }

    public class SaveSoundFileService : ISaveSoundFileService
    {
        private readonly IDeviceInfo _deviceInfo;
        private readonly IFileDownloaderService _fileDownloaderService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSaver _fileSaver;
        private readonly IFileIOService _fileIOService;
        private readonly IDialogService _dialogService;
        private readonly IGlobalSettings _globalSettings;

        public SaveSoundFileService(
            IDeviceInfo deviceInfo,
            IFileDownloaderService fileDownloaderService,
            ISettingsService settingsService,
            IFileSaver fileSaver,
            IFileIOService fileIOService,
            IDialogService dialogService,
            IGlobalSettings globalSettings)
        {
            _deviceInfo = deviceInfo;
            _fileDownloaderService = fileDownloaderService;
            _fileSaver = fileSaver;
            _settingsService = settingsService;
            _fileIOService = fileIOService;
            _dialogService = dialogService;
            _globalSettings = globalSettings;
        }

        public string CreateDownloadSoundFileUrl(string soundUrl, string word)
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v1/Sound/DownloadSound?" +
                   $"soundUrl={Uri.EscapeDataString(soundUrl)}&word={Uri.EscapeDataString(word)}&version=1&code={_globalSettings.TranslatorAppRequestCode}";
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [SupportedOSPlatform("android")]
        public async Task<bool> SaveSoundFileAsync(string url, string word, CancellationToken cancellationToken)
        {
            // on Android show the FileSavePicker and save the file into allowed location, like Downloads
            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                return await DownloadSoundFileAndSaveWithFileSaverAsync(url, word, cancellationToken);
            }

            // On Windows and Mac, download the file and save it directly to the Anki collection media folder.
            return await DownloadSoundFileAndCopyToAnkiFolderAsync(url, word, cancellationToken);
        }

        internal async Task<bool> DownloadSoundFileAndCopyToAnkiFolderAsync(string soundUrl, string word, CancellationToken cancellationToken)
        {
            string ankiSoundsFolder = _settingsService.LoadSettings().AnkiSoundsFolder;
            if (!_fileIOService.DirectoryExists(ankiSoundsFolder))
            {
                await _dialogService.DisplayAlertAsync("Path to Anki folder is incorrect", $"Cannot find path to Anki folder '{ankiSoundsFolder}'. Please update it in Settings.", "OK");
                return false;
            }

            string downloadSoundUrl = CreateDownloadSoundFileUrl(soundUrl, word);
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

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [SupportedOSPlatform("android")]
        internal async Task<bool> DownloadSoundFileAndSaveWithFileSaverAsync(string soundUrl, string word, CancellationToken cancellationToken)
        {
            string downloadSoundUrl = CreateDownloadSoundFileUrl(soundUrl, word);
            using Stream stream = await _fileDownloaderService.DownloadFileAsync(downloadSoundUrl, cancellationToken);

            var fileSaverResult = await _fileSaver.SaveAsync($"{word}.mp3", stream, cancellationToken);
            return fileSaverResult.IsSuccessful;
        }
    }
}
