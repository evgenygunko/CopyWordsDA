using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace CopyWords.Core.Services
{
    public interface ISaveSoundFileService
    {
        Task<bool> SaveSoundFileAsync(string url, string soundFileName);
    }

    public class SaveSoundFileService : SaveFileServiceBase, ISaveSoundFileService
    {
        private readonly IClipboardService _clipboardService;

        public SaveSoundFileService(
            ISettingsService settingsService,
            HttpClient httpClient,
            IDialogService dialogService,
            IClipboardService clipboardService)
            : base(settingsService, httpClient, dialogService)
        {
            _clipboardService = clipboardService;
        }

        #region Public Methods

        public async Task<bool> SaveSoundFileAsync(string url, string soundFileName)
        {
            // download file from web into temp folder
            string mp3File = await DownloadFileAsync(url, soundFileName);
            if (string.IsNullOrEmpty(mp3File))
            {
                return false;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (_settingsService.UseMp3gain && !await CallMp3gainAsync(mp3File))
                {
                    return false;
                }
            }

            // save text for Anki into clipboard
            string clipboardTest = $"[sound:{Path.GetFileNameWithoutExtension(mp3File)}.mp3]";
            await _clipboardService.CopyTextToClipboardAsync(clipboardTest);

            // copy file into Anki's sounds folder
            if (!await CopyFileToAnkiFolderAsync(mp3File))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Private Methods

        [SupportedOSPlatform("windows")]
        private async Task<bool> CallMp3gainAsync(string sourceMp3File)
        {
            Debug.Assert(File.Exists(sourceMp3File));

            string mp3gainPath = _settingsService.GetMp3gainPath();
            if (!File.Exists(mp3gainPath))
            {
                await _dialogService.DisplayAlert("Cannot find path to mp3gain", $"Cannot find mp3gain by '{mp3gainPath}'. Please update it in Settings.", "OK");
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
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("Cannot normalize mp3 file", ex.Message, "OK");
                return false;
            }

            File.Copy(tempAsciiFile, sourceMp3File, true);
            File.Delete(tempAsciiFile);

            return true;
        }

        #endregion
    }
}
