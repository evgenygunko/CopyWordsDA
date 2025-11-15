using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Models;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class GetUpdateViewModel : ObservableObject
    {
        private readonly IUpdateService _updateService;
        private readonly IShellService _shellService;
        private readonly IDialogService _dialogService;

        public GetUpdateViewModel(
            IUpdateService updateService,
            IShellService shellService,
            IDialogService dialogService)
        {
            _updateService = updateService;
            _shellService = shellService;
            _dialogService = dialogService;
            WhatIsNew = string.Empty;
            UpdateDescription = string.Empty;
            CurrentVersion = string.Empty;
            LatestVersion = string.Empty;
            ErrorMessage = string.Empty;
            DownloadUrl = string.Empty;
        }

        [ObservableProperty]
        public partial string WhatIsNew { get; set; }

        [ObservableProperty]
        public partial string UpdateDescription { get; set; }

        [ObservableProperty]
        public partial string CurrentVersion { get; set; }

        [ObservableProperty]
        public partial string LatestVersion { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string DownloadUrl { get; set; }

        [RelayCommand]
        public async Task GetLatestReleaseAsync()
        {
            try
            {
                // This command is called from "Appearing" event handler.
                ReleaseInfo releaseInfo = await _updateService.GetLatestReleaseVersionAsync();

                WhatIsNew = $"What is new in {releaseInfo.LatestVersion}";
                UpdateDescription = releaseInfo.Description;
                CurrentVersion = AppInfo.VersionString;
                LatestVersion = releaseInfo.LatestVersion;
                DownloadUrl = releaseInfo.DownloadUrl;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while getting the latest release version. Error: " + ex);
                ErrorMessage = "An error occurred while checking for updates. Error: " + ex.Message;
            }
        }

        [RelayCommand]
        public async Task DownloadUpdateAsync()
        {
            Uri uri = new Uri(DownloadUrl);

            try
            {
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
                await _dialogService.DisplayAlertAsync("Download update", "After downloading the update, close this program and click on the downloaded file.", "OK");
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
                Debug.WriteLine($"Cannot open the browser page '{uri}'. Error: " + ex);
            }
        }

        [RelayCommand]
        public async Task CloseDialogAsync()
        {
            await _shellService.PopModalAsync();
        }
    }
}
