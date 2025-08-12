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
        private readonly IInstantTranslationService _instantTranslationService;

        public HistoryPageViewModel(
            ISettingsService settingsService,
            IShellService shellService,
            IDialogService dialogService,
            IInstantTranslationService instantTranslationService)
        {
            _settingsService = settingsService;
            _shellService = shellService;
            _dialogService = dialogService;
            _instantTranslationService = instantTranslationService;
        }

        public ObservableCollection<PreviousWordViewModel> PreviousWords { get; } = [];

        [RelayCommand]
        public void Init()
        {
            PreviousWords.Clear();

            string dictionary = _settingsService.GetSelectedParser();
            foreach (string item in _settingsService.LoadHistory(dictionary))
            {
                var previousWordViewModelVM = new PreviousWordViewModel(item);
                previousWordViewModelVM.Clicked += async (sender, word) =>
                {
                    Debug.WriteLine($"Word clicked, will lookup '{word}'");

                    // One approach would be to pass navigation state to the MainViewModel and bind the query parameter "word" to the MainViewModel.SearchWord property.
                    // But then we would need to update the logic in InitAsync and add more conditions.
                    // Instead, we can just set the text in the InstantTranslationService and it will work the same way as if the MainView model was called from the context menu on Android.
                    _instantTranslationService.SetText(word);
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
