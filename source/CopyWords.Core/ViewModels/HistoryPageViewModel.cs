using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class HistoryPageViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IShellService _shellService;
        private readonly IDialogService _dialogService;

        public HistoryPageViewModel(
            ISettingsService settingsService,
            IShellService shellService,
            IDialogService dialogService)
        {
            _settingsService = settingsService;
            _shellService = shellService;
            _dialogService = dialogService;
        }

        public ObservableCollection<PreviousWordViewModel> PreviousWords { get; } = [];

        [RelayCommand]
        public void Init()
        {
            PreviousWords.Clear();

            string dictionary = _settingsService.GetSelectedParser();
            foreach (string item in _settingsService.LoadHistory(dictionary))
            {
                var previousWordViewModelVM = new PreviousWordViewModel(item, "https://ordnet.dk/ddo/ordbog?select=ramme,4&query=ramme");
                previousWordViewModelVM.Clicked += async (sender, url) =>
                {
                    Debug.WriteLine($"Word clicked, will lookup '{url}'");
                    await _shellService.GoToAsync("..");
                };

                PreviousWords.Add(previousWordViewModelVM);
            }
        }

        [RelayCommand]
        public async Task ClearHistoryAsync()
        {
            try
            {
                PreviousWords.Clear();

                string dictionary = _settingsService.GetSelectedParser();
                _settingsService.ClearHistory(dictionary);
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("Cannot clear history", $"Cannot clear history. Error: {ex}", "OK");
            }
        }

        [RelayCommand]
        public async Task CancelAsync()
        {
            await _shellService.GoToAsync("..");
        }
    }
}
