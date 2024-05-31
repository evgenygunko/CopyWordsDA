using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ExampleViewModel : ObservableObject
    {
        public ExampleViewModel(Example example)
        {
            ExampleES = example?.ExampleES;
            ExampleEN = example?.ExampleEN;
        }

        [ObservableProperty]
        private string exampleES;

        [ObservableProperty]
        private string exampleEN;

        [ObservableProperty]
        private bool isChecked;
    }
}
