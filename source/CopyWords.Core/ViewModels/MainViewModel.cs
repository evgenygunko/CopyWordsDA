using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Parsers;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private ILookUpWord _lookUpWord;
        private WordViewModel _wordViewModel;

        public MainViewModel(ILookUpWord lookUpWord, WordViewModel wordViewModel)
        {
            _lookUpWord = lookUpWord;
            _wordViewModel = wordViewModel;
        }

        #region Properties

        public WordViewModel WordViewModel => _wordViewModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private bool isBusy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LookUpCommand))]
        private string searchWord;

        public bool CanExecuteLookUp => !IsBusy && !string.IsNullOrWhiteSpace(SearchWord);

        public bool CanShowSettingsDialog => !IsBusy;

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanExecuteLookUp))]
        public async Task LookUp()
        {
            IsBusy = true;

            WordModel wordModel = await LookUpWordAsync(SearchWord);

            if (wordModel != null)
            {
                _wordViewModel.Front = wordModel.Headword;
                _wordViewModel.PartOfSpeech = wordModel.PartOfSpeech;
                _wordViewModel.Forms = wordModel.Endings;
                _wordViewModel.SoundUrl = wordModel.SoundUrl;
                _wordViewModel.SoundFileName = wordModel.SoundFileName;

                _wordViewModel.Definitions.Clear();
                int i = 1;
                foreach (var definition in wordModel.Definitions)
                {
                    _wordViewModel.Definitions.Add(new DefinitionViewModel(definition, i++));
                }
            }

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanShowSettingsDialog))]
        public async Task ShowSettingsDialog()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }

        #endregion

        #region Private Methods

        private async Task<WordModel> LookUpWordAsync(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return null;
            }

            (bool isValid, string errorMessage) = _lookUpWord.CheckThatWordIsValid(word);
            if (!isValid)
            {
                await Shell.Current.DisplayAlert("Invalid search term", errorMessage, "OK");
                return null;
            }

            WordModel wordModel = null;
            try
            {
                wordModel = await _lookUpWord.LookUpWordAsync(word);

                if (wordModel == null)
                {
                    await Shell.Current.DisplayAlert("Cannot find word", $"Could not find a translation for '{word}'", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error occurred while searching translations", ex.Message, "OK");
                return null;
            }

            return wordModel;
        }

        #endregion
    }
}
