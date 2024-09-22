using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CopyWords.Parsers.Models;

namespace CopyWords.Core.ViewModels
{
    public partial class DefinitionViewModel : ObservableObject
    {
        public DefinitionViewModel(Definition definition, int pos)
        {
            Meaning meaning = definition.Meanings.First();

            Position = pos + ". ";
            Tag = meaning.Tag?.ToUpper();
            Meaning = meaning.Description;

            Examples.Clear();
            foreach (Example example in meaning.Examples)
            {
                Examples.Add(new ExampleViewModel(example.Original));
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
