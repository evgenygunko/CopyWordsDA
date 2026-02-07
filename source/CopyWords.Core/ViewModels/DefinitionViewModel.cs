using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public event EventHandler<string> MeaningLookupClicked = default!;

        public DefinitionViewModel(
            Definition definition,
            SourceLanguage sourceLanguage,
            bool showCheckBoxes)
        {
            HeadwordViewModel = new HeadwordViewModel(definition.Headword, sourceLanguage, showCheckBoxes);

            PartOfSpeech = definition.PartOfSpeech;
            Endings = definition.Endings;

            ContextViewModels.Clear();
            foreach (var context in definition.Contexts)
            {
                var contextVM = new ContextViewModel(context, sourceLanguage, showCheckBoxes);
                contextVM.MeaningLookupClicked += (sender, url) => MeaningLookupClicked?.Invoke(sender, url);
                ContextViewModels.Add(contextVM);
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

        public ObservableCollection<ContextViewModel> ContextViewModels { get; } = [];

        #endregion
    }
}
