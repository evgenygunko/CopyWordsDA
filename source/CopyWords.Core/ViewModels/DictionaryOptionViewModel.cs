using CommunityToolkit.Mvvm.ComponentModel;

namespace CopyWords.Core.ViewModels
{
    public partial class DictionaryOptionViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string LanguageKey { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsEnabled { get; set; }
    }
}
