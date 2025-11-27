// Ignore Spelling: Downloader

using System.Runtime.Versioning;
using CommunityToolkit.Maui.Storage;
using CopyWords.Core.Services.Wrappers;

namespace CopyWords.Core.Services
{
    public interface ISaveSoundFileService
    {
        Task<bool> SaveSoundFileAsync(string url, string soundFileName, CancellationToken cancellationToken);
    }

    public class SaveSoundFileService : ISaveSoundFileService
    {
        private readonly IClipboardService _clipboardService;
        private readonly IDeviceInfo _deviceInfo;
        private readonly IFileDownloaderService _fileDownloaderService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSaver _fileSaver;
        private readonly IFileIOService _fileIOService;
        private readonly IDialogService _dialogService;

        public SaveSoundFileService(
            IClipboardService clipboardService,
            IDeviceInfo deviceInfo,
            IFileDownloaderService fileDownloaderService,
            ISettingsService settingsService,
            IFileSaver fileSaver,
            IFileIOService fileIOService,
            IDialogService dialogService)
        {
            _clipboardService = clipboardService;
            _deviceInfo = deviceInfo;
            _fileDownloaderService = fileDownloaderService;
            _fileSaver = fileSaver;
            _settingsService = settingsService;
            _fileIOService = fileIOService;
            _dialogService = dialogService;
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [SupportedOSPlatform("android")]
        public async Task<bool> SaveSoundFileAsync(string url, string soundFileName, CancellationToken cancellationToken)
        {
            // todo: pass the word from the viewmodel instead of extracting it from the file name
            string word = Path.GetFileNameWithoutExtension(soundFileName);

            // on Android show the FileSavePicker and save the file into allowed location, like Downloads
            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                return await DownloadSoundFileAndSaveWithFileSaverAsync(url, word, cancellationToken);
            }

            // Put the text for Anki into the clipboard. We do this before attempting to save the actual file.
            // If the file already exists and the user chooses not to overwrite it, the text will still be available in the clipboard.
            string clipboardTest = $"[sound:{word}.mp3]";
            await _clipboardService.CopyTextToClipboardAsync(clipboardTest);

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

            using Stream stream = await _fileDownloaderService.DownloadSoundFileAsync(soundUrl, word, cancellationToken);

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
            using Stream stream = await _fileDownloaderService.DownloadSoundFileAsync(soundUrl, word, cancellationToken);

            var fileSaverResult = await _fileSaver.SaveAsync($"{word}.mp3", stream, cancellationToken);
            return fileSaverResult.IsSuccessful;
        }
    }
}
