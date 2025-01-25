// Ignore Spelling: popup Popups

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels.Popups
{
    public partial class SelectDictionaryPopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;

        public SelectDictionaryPopupViewModel(IPopupService popupService)
        {
            _popupService = popupService;

            IsSpanishDictChecked = true;
        }

        [ObservableProperty]
        private bool isDDOChecked;

        [ObservableProperty]
        private bool isSpanishDictChecked;

        [RelayCommand]
        public void Cancel()
        {
            _popupService.ClosePopup();
        }

        [RelayCommand]
        public void Save()
        {
            SourceLanguage sourceLanguage = IsDDOChecked ? SourceLanguage.Danish : SourceLanguage.Spanish;
            _popupService.ClosePopup(sourceLanguage);
        }
    }
}
