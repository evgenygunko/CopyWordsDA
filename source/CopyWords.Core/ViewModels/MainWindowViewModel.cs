using CommunityToolkit.Mvvm.ComponentModel;

namespace CopyWords.Core.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public MainWindowViewModel()
        {
            Title = "CopyWords";
            Subtitle = "";
        }

        #region Properties

        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial string Subtitle { get; set; }

        #endregion
    }
}