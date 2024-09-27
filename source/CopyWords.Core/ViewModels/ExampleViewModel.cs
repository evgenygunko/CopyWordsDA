using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class ExampleViewModel : ObservableObject
    {
        public ExampleViewModel(Example example)
        {
            Original = example?.Original;
            English = example?.English;
        }

        [ObservableProperty]
        private string original;

        [ObservableProperty]
        private string english;

        [ObservableProperty]
        private bool isChecked;
    }
}
