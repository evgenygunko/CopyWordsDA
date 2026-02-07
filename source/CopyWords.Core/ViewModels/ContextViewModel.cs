using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ContextViewModel : ObservableObject
    {
        public event EventHandler<string> MeaningLookupClicked = default!;

        public ContextViewModel(
            Context context,
            SourceLanguage sourceLanguage,
            bool showCheckBoxes)
        {
            ContextEN = context.ContextEN;
            Position = context.Position;

            MeaningViewModels.Clear();
            foreach (var meanings in context.Meanings)
            {
                var meaningVM = new MeaningViewModel(meanings, sourceLanguage, showCheckBoxes);
                meaningVM.MeaningLookupClicked += (sender, url) => MeaningLookupClicked?.Invoke(sender, url);
                MeaningViewModels.Add(meaningVM);
            }
        }

        [ObservableProperty]
        public partial string ContextEN { get; set; }

        [ObservableProperty]
        public partial string Position { get; set; }

        public ObservableCollection<MeaningViewModel> MeaningViewModels { get; } = [];
    }
}
