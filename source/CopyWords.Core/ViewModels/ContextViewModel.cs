using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Core.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ContextViewModel : ObservableObject
    {
        public ContextViewModel(Context context, bool showTranslatedMeanings)
        {
            ContextEN = context.ContextEN;
            Position = context.Position;

            MeaningViewModels.Clear();
            foreach (var meanings in context.Meanings)
            {
                MeaningViewModels.Add(new MeaningViewModel(meanings, showTranslatedMeanings));
            }
        }

        [ObservableProperty]
        private string contextEN;

        [ObservableProperty]
        private string position;

        public ObservableCollection<MeaningViewModel> MeaningViewModels { get; } = new();
    }
}
