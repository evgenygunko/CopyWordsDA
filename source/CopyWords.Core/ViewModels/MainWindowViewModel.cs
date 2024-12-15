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
        private string title;

        [ObservableProperty]
        private string subtitle;

        #endregion
    }
}