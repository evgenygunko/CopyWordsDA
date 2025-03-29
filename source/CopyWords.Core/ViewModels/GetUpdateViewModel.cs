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

            whatIsNew = string.Empty;
            updateDescription = string.Empty;
            currentVersion = string.Empty;
            latestVersion = string.Empty;
            errorMessage = string.Empty;
            downloadUrl = string.Empty;
        }

        [ObservableProperty]
        private string whatIsNew;

        [ObservableProperty]
        private string updateDescription;

        [ObservableProperty]
        private string currentVersion;

        [ObservableProperty]
        private string latestVersion;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string downloadUrl;

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
                await _dialogService.DisplayAlert("Download update", "After downloading the update, close this program and click on the downloaded file.", "OK");
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
