using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ContextViewModel : ObservableObject
    {
        public ContextViewModel(Context context)
        {
            ContextEN = context?.ContextEN;
            Position = context.Position;

            MeaningViewModels.Clear();
            foreach (var meanings in context.Meanings)
            {
                MeaningViewModels.Add(new MeaningViewModel(meanings));
            }
        }

        [ObservableProperty]
        private string contextEN;

        [ObservableProperty]
        private int position;

        public ObservableCollection<MeaningViewModel> MeaningViewModels { get; } = new();
    }
}
