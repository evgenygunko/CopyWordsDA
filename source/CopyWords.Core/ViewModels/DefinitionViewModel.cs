using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public DefinitionViewModel(Definition definition)
        {
            Meaning = definition.Meaning;

            Examples.Clear();
            foreach (string example in definition.Examples)
            {
                Examples.Add(new ExampleViewModel(example));
            }
        }

        [ObservableProperty]
        private string meaning;

        [ObservableProperty]
        private string alphabeticalPosition;

        public ObservableCollection<ExampleViewModel> Examples { get; } = new();
    }
}
