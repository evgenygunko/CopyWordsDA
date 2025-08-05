using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ContextViewModel : ObservableObject
    {
        public ContextViewModel(
            Context context,
            SourceLanguage sourceLanguage,
            bool showCopyButtons)
        {
            ContextEN = context.ContextEN;
            Position = context.Position;

            MeaningViewModels.Clear();
            foreach (var meanings in context.Meanings)
            {
                MeaningViewModels.Add(new MeaningViewModel(meanings, sourceLanguage, showCopyButtons));
            }
        }

        [ObservableProperty]
        public partial string ContextEN { get; set; }

        [ObservableProperty]
        public partial string Position { get; set; }

        public ObservableCollection<MeaningViewModel> MeaningViewModels { get; } = [];
    }
}
