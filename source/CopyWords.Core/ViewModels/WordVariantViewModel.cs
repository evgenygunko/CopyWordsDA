using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class WordVariantViewModel : ObservableObject
    {
        public WordVariantViewModel(WordVariant wordVariant)
        {
            WordES = wordVariant?.WordES;
            WordType = wordVariant.Type;

            ContextViewModels.Clear();
            foreach (var context in wordVariant.Contexts)
            {
                ContextViewModels.Add(new ContextViewModel(context));
            }
        }

        [ObservableProperty]
        private string wordES;

        [ObservableProperty]
        private string wordType;

        public ObservableCollection<ContextViewModel> ContextViewModels { get; } = new();
    }
}
