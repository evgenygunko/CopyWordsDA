using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public DefinitionViewModel(Definition definition, int pos)
        {
            Meaning = definition.Meaning;
            Position = pos;

            Examples.Clear();
            foreach (string example in definition.Examples)
            {
                Examples.Add(new ExampleViewModel(example));
            }
        }

        [ObservableProperty]
        private string meaning;

        [ObservableProperty]
        private int position;

        public ObservableCollection<ExampleViewModel> Examples { get; } = new();
    }
}
