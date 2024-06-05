using CommunityToolkit.Mvvm.ComponentModel;

namespace CopyWords.Core.ViewModels
{
    public partial class ExampleViewModel : ObservableObject
    {
        public ExampleViewModel(string value)
        {
            Example = value;
        }

        [ObservableProperty]
        private string example;

        [ObservableProperty]
        private bool isChecked;
    }
}
