// Ignore Spelling: Downloader

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using CommunityToolkit.Maui.Storage;
using FFMpegCore;

namespace CopyWords.Core.Services
{
    public interface ISaveSoundFileService
    {
        Task<bool> SaveSoundFileAsync(string url, string soundFileName, CancellationToken cancellationToken);
    }

    public class SaveSoundFileService : ISaveSoundFileService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;
        private readonly IDeviceInfo _deviceInfo;
        private readonly IFileSaver _fileSaver;
        private readonly IFileDownloaderService _fileDownloaderService;
        private readonly ILaunchDarklyService _launchDarklyService;

        public SaveSoundFileService(
            HttpClient httpClient,
            ISettingsService settingsService,
            IDialogService dialogService,
            IClipboardService clipboardService,
            IFileIOService fileIOService,
            IDeviceInfo deviceInfo,
            IFileSaver fileSaver,
            IFileDownloaderService fileDownloaderService,
            ILaunchDarklyService launchDarklyService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
            _deviceInfo = deviceInfo;
            _fileSaver = fileSaver;
            _fileDownloaderService = fileDownloaderService;
            _launchDarklyService = launchDarklyService;
        }

        #region Public Methods

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [SupportedOSPlatform("android")]
        public async Task<bool> SaveSoundFileAsync(string url, string soundFileName, CancellationToken cancellationToken)
        {
            bool downloadSoundFromTranslationAppFlag = _launchDarklyService.GetBooleanFlag("download-sound-from-translation-app", false);

            // on Android show the FileSavePicker and save the file into allowed location, like Downloads
            if (_deviceInfo.Platform == DevicePlatform.Android)
            {
                return await SaveFileWithFileSaverAsync(url, soundFileName, cancellationToken);
            }

            // On Windows and Mac, download the file and save it to the Anki collection media folder.
            // First, download the file from the web into a temporary folder.
            string? soundFileFullPath = await _fileDownloaderService.DownloadFileAsync(url, soundFileName);
            if (string.IsNullOrEmpty(soundFileFullPath))
            {
                return false;
            }

            string extension = Path.GetExtension(soundFileFullPath);
            if (string.Equals(extension, ".mp4", StringComparison.InvariantCultureIgnoreCase))
            {
                // transcode mp4 to mp3
                soundFileFullPath = await TranscodeToMp3Async(soundFileFullPath);
                if (string.IsNullOrEmpty(soundFileFullPath))
                {
                    return false;
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (_settingsService.LoadSettings().UseMp3gain && !await CallMp3gainAsync(soundFileFullPath))
                {
                    return false;
                }
            }

            // Put the text for Anki into the clipboard. We do this before attempting to save the actual file.
            // If the file already exists and the user chooses not to overwrite it, the text will still be available in the clipboard.
            string clipboardTest = $"[sound:{Path.GetFileNameWithoutExtension(soundFileFullPath)}.mp3]";
            await _clipboardService.CopyTextToClipboardAsync(clipboardTest);

            // copy file into the Anki collection media folder
            if (!await _fileDownloaderService.CopyFileToAnkiFolderAsync(soundFileFullPath))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Internal Methods

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("maccatalyst15.0")]
        [SupportedOSPlatform("android")]
        internal async Task<bool> SaveFileWithFileSaverAsync(string url, string soundFileName, CancellationToken cancellationToken)
        {
            using var ctsHttpRequest = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ctsHttpRequest.Token, cancellationToken);

            await using var stream = await _httpClient.GetStreamAsync(url, ctsHttpRequest.Token);

            var fileSaverResult = await _fileSaver.SaveAsync(soundFileName, stream, cancellationToken);
            return fileSaverResult.IsSuccessful;
        }

        #endregion

        #region Private Methods

        private async Task<string?> TranscodeToMp3Async(string mp4File)
        {
            Debug.Assert(File.Exists(mp4File));

            string soundName = Path.GetFileNameWithoutExtension(mp4File);
            string destFileFullPath = Path.Combine(Path.GetDirectoryName(mp4File)!, $"{soundName}.mp3");

            string ffmpegBinFolder = _settingsService.LoadSettings().FfmpegBinFolder;
            if (!Directory.Exists(ffmpegBinFolder))
            {
                await _dialogService.DisplayAlertAsync("Cannot find ffmpeg bin folder", $"Cannot find ffmpeg bin folder '{ffmpegBinFolder}'. Please update it in Settings.", "OK");
                return null;
            }

            GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegBinFolder);
            FFMpeg.ExtractAudio(mp4File, destFileFullPath);

            return destFileFullPath;
        }

        [SupportedOSPlatform("windows")]
        private async Task<bool> CallMp3gainAsync(string sourceMp3File)
        {
            Debug.Assert(File.Exists(sourceMp3File));

            string mp3gainPath = _settingsService.LoadSettings().Mp3gainPath;
            if (!File.Exists(mp3gainPath))
            {
                await _dialogService.DisplayAlertAsync("Cannot find path to mp3gain", $"Cannot find mp3gain by '{mp3gainPath}'. Please update it in Settings.", "OK");
                return false;
            }

            // m3gain doesn't support unicode file names - we will use a temp name and then rename it back to desired file name
            string tempAsciiFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp3");
            File.Copy(sourceMp3File, tempAsciiFile);

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = mp3gainPath;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = "-r " + tempAsciiFile;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process? exeProcess = Process.Start(startInfo))
                {
                    exeProcess?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync("Cannot normalize mp3 file", ex.Message, "OK");
                return false;
            }

            File.Copy(tempAsciiFile, sourceMp3File, true);
            File.Delete(tempAsciiFile);

            return true;
        }

        #endregion
    }
}
