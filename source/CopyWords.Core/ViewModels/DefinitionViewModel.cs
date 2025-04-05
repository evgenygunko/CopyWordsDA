using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Services;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public DefinitionViewModel(
            string word,
            Definition definition,
            ISettingsService settingsService)
        {
            Word = word;
            HeadwordViewModel = new HeadwordViewModel(definition.Headword, settingsService);

            PartOfSpeech = definition.PartOfSpeech;
            Endings = definition.Endings;

            ContextViewModels.Clear();
            foreach (var context in definition.Contexts)
            {
                ContextViewModels.Add(new ContextViewModel(context));
            }
        }

        #region Properties

        [ObservableProperty]
        private string word;

        [ObservableProperty]
        private HeadwordViewModel headwordViewModel;

        [ObservableProperty]
        private string partOfSpeech;

        [ObservableProperty]
        private string endings;

        #endregion

        #region Commands

        public ObservableCollection<ContextViewModel> ContextViewModels { get; } = new();

        #endregion
    }
}
