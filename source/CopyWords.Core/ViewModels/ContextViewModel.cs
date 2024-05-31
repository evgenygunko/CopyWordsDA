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

            TranslationViewModels.Clear();
            foreach (var translation in context.Translations)
            {
                TranslationViewModels.Add(new TranslationViewModel(translation));
            }
        }

        [ObservableProperty]
        private string contextEN;

        [ObservableProperty]
        private int position;

        public ObservableCollection<TranslationViewModel> TranslationViewModels { get; } = new();
    }
}
