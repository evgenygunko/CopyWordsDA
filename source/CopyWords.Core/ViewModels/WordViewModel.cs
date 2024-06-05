﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class WordViewModel : ObservableObject
    {
        private readonly ISaveSoundFileService _saveSoundFileService;
        private readonly ICopySelectedToClipboardService _copySelectedToClipboardService;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;

        public WordViewModel(
            ISaveSoundFileService saveSoundFileService,
            ICopySelectedToClipboardService copySelectedToClipboardService,
            IDialogService dialogService,
            IClipboardService clipboardService)
        {
            _saveSoundFileService = saveSoundFileService;
            _copySelectedToClipboardService = copySelectedToClipboardService;
            _dialogService = dialogService;
            _clipboardService = clipboardService;
        }

        #region Properties

        public ObservableCollection<DefinitionViewModel> Definitions { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopyFrontCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyExamplesCommand))]
        private string front = "<>";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private string _soundUrl;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyPropertyChangedFor(nameof(SaveSoundButtonColor))]
        private string _soundFileName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PlaySoundCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveSoundFileCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyFrontCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyBackCommand))]
        [NotifyCanExecuteChangedFor(nameof(CopyExamplesCommand))]
        [NotifyPropertyChangedFor(nameof(PlaySoundButtonColor))]
        private bool isBusy;

        public bool CanSaveSoundFile => !IsBusy && !string.IsNullOrEmpty(SoundFileName);

        public bool CanPlaySound => !IsBusy && !string.IsNullOrEmpty(SoundUrl);

        public bool CanCopyFront => !IsBusy && Front != "<>";

        public Color PlaySoundButtonColor => GetButtonColor(CanPlaySound);

        public Color SaveSoundButtonColor => GetButtonColor(CanSaveSoundFile);

        public Color CopyFrontButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyBackButtonColor => GetButtonColor(CanCopyFront);

        public Color CopyExamplesButtonColor => GetButtonColor(CanCopyFront);

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanPlaySound))]
        public void PlaySound(object control)
        {
            IsBusy = true;

            Debug.WriteLine("Will play " + SoundUrl);

            MediaElement mediaElement = (MediaElement)control;
            mediaElement.Source = MediaSource.FromUri(SoundUrl);
            mediaElement.Play();

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanSaveSoundFile))]
        public async Task SaveSoundFileAsync()
        {
            IsBusy = true;

            try
            {
                bool result = await _saveSoundFileService.SaveSoundFileAsync(SoundUrl, SoundFileName);

                if (result)
                {
                    await _dialogService.DisplayToast("Sound file saved");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert("Cannot save sound file", "Error occurred while trying to save sound file: " + ex.Message, "OK");
            }

            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyFrontAsync()
        {
            await _dialogService.DisplayAlert("Not implemented yet", "Copy command is not implemented yet", "OK");
            /*await CompileAndCopyToClipboard("front", _copySelectedToClipboardService.CompileFrontAsync);*/
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyBackAsync()
        {
            await _dialogService.DisplayAlert("Not implemented yet", "Copy command is not implemented yet", "OK");
            /*await CompileAndCopyToClipboard("back", _copySelectedToClipboardService.CompileBackAsync);*/
        }

        [RelayCommand(CanExecute = nameof(CanCopyFront))]
        public async Task CopyExamplesAsync()
        {
            await _dialogService.DisplayAlert("Not implemented yet", "Copy command is not implemented yet", "OK");
            /*await CompileAndCopyToClipboard("examples", _copySelectedToClipboardService.CompileExamplesAsync);*/
        }

        #endregion

        #region Internal methods

        internal async Task CompileAndCopyToClipboard(string wordPartName, Func<ObservableCollection<DefinitionViewModel>, Task<string>> action)
        {
            _ = wordPartName;
            _ = action;
            await _dialogService.DisplayAlert("Not implemented yet", "Copy command is not implemented yet", "OK");
            /*try
            {
                string textToCopy = await action(WordVariantViewModels);

                if (!string.IsNullOrEmpty(textToCopy))
                {
                    await _clipboardService.CopyTextToClipboardAsync(textToCopy);
                    await _dialogService.DisplayToast(string.Concat(wordPartName[0].ToString().ToUpper(CultureInfo.CurrentCulture), wordPartName.AsSpan(1), " copied"));
                }
                else
                {
                    await _dialogService.DisplayAlert("Text not copied", "Please select at least one example", "OK");
                }
            }
            catch (PrepareWordForCopyingException ex)
            {
                await _dialogService.DisplayAlert($"Cannot copy {wordPartName}", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlert($"Cannot copy {wordPartName}", $"Error occurred while trying to copy {wordPartName}: " + ex.Message, "OK");
            }*/
        }

        private static Color GetButtonColor(bool isEnabled)
        {
            Color color = isEnabled ? Color.Parse("#512BD4") : Color.Parse("#919191");
            return color;
        }

        #endregion
    }
}
