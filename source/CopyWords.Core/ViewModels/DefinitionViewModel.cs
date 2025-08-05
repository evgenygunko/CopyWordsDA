using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public DefinitionViewModel(
            Definition definition,
            SourceLanguage sourceLanguage,
            bool showCopyButtons)
        {
            HeadwordViewModel = new HeadwordViewModel(definition.Headword, sourceLanguage, showCopyButtons);

            PartOfSpeech = definition.PartOfSpeech;
            Endings = definition.Endings;

            ContextViewModels.Clear();
            foreach (var context in definition.Contexts)
            {
                ContextViewModels.Add(new ContextViewModel(context, sourceLanguage, showCopyButtons));
            }
        }

        #region Properties

        [ObservableProperty]
        public partial HeadwordViewModel HeadwordViewModel { get; set; }

        [ObservableProperty]
        public partial string PartOfSpeech { get; set; }

        [ObservableProperty]
        public partial string Endings { get; set; }

        #endregion

        #region Commands

        public ObservableCollection<ContextViewModel> ContextViewModels { get; } = new();

        #endregion
    }
}
