using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public DefinitionViewModel(Definition definition, int pos)
        {
            Position = pos + ". ";
            Tag = definition.Tag;
            Meaning = definition.Meaning;

            Examples.Clear();
            foreach (string example in definition.Examples)
            {
                Examples.Add(new ExampleViewModel(example));
            }

            if (Examples.Count == 0)
            {
                Examples.Add(new ExampleViewModel("-"));
            }
        }

        [ObservableProperty]
        public string position;

        [ObservableProperty]
        private string tag;

        [ObservableProperty]
        private string meaning;

        public ObservableCollection<ExampleViewModel> Examples { get; } = new();
    }
}
