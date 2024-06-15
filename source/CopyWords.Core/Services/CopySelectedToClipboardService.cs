using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CopyWords.Core.ViewModels;

namespace CopyWords.Core.Services
{
    public interface ICopySelectedToClipboardService
    {
        Task<string> CompileFrontAsync(string meaning, string partOfSpeech);

        Task<string> CompileBackAsync(ObservableCollection<DefinitionViewModel> definitionVMs);

        Task<string> CompileFormsAsync(string forms);

        Task<string> CompileExamplesAsync(ObservableCollection<DefinitionViewModel> definitionVMs);
    }

    public class CopySelectedToClipboardService : ICopySelectedToClipboardService
    {
        private readonly ISaveImageFileService _saveImageFileService;

        private const string TemplateGrayText = "<span style=\"color: rgba(0, 0, 0, 0.4)\">{0}</span>";

        public CopySelectedToClipboardService(ISaveImageFileService saveImageFileService)
        {
            _saveImageFileService = saveImageFileService;
        }

        #region Public Methods

        public Task<string> CompileFrontAsync(string meaning, string partOfSpeech)
        {
            string front = meaning;

            if (!string.IsNullOrEmpty(partOfSpeech))
            {
                if (partOfSpeech.Equals("substantiv, intetkøn", StringComparison.OrdinalIgnoreCase))
                {
                    front = $"et {front}";
                }
                if (partOfSpeech.Equals("substantiv, fælleskøn", StringComparison.OrdinalIgnoreCase))
                {
                    front = $"en {front}";
                }
                if (partOfSpeech.Equals("verbum", StringComparison.OrdinalIgnoreCase))
                {
                    front = $"at {front}";
                }
                if (partOfSpeech.Equals("adjektiv", StringComparison.OrdinalIgnoreCase))
                {
                    front = front + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, partOfSpeech.ToUpper());
                }
            }

            return Task.FromResult(front);
        }

        public Task<string> CompileBackAsync(ObservableCollection<DefinitionViewModel> definitionVMs)
        {
            if (definitionVMs == null)
            {
                return Task.FromResult(string.Empty);
            }

            List<string> meanings = new();

            foreach (var definitionVM in definitionVMs)
            {
                // The meaning is the same for all examples - so find first example which is selected
                ExampleViewModel exampleVM = definitionVM.Examples.FirstOrDefault(x => x.IsChecked);
                if (exampleVM != null)
                {
                    string htmlMeaning = string.Empty;
                    if (!string.IsNullOrEmpty(definitionVM.Tag))
                    {
                        htmlMeaning = $"<span style=\"color:#404040; background-color:#eaeff2; border:1px solid #CCCCCC; margin-right:10px; font-size: 80%;\">{definitionVM.Tag}</span>";
                    }

                    htmlMeaning += definitionVM.Meaning;

                    meanings.Add(htmlMeaning);
                }
            }

            // Now go through all meanings and add numbering
            StringBuilder sb = new StringBuilder();
            int count = meanings.Count;
            int i = 1;

            foreach (string meaning in meanings)
            {
                if (count > 1)
                {
                    sb.Append($"{i}.&nbsp;");
                }

                sb.Append(meaning);
                sb.Append("<br>");
                i++;
            }

            return Task.FromResult(sb.ToString().TrimEnd("<br>".ToCharArray()));
        }

        public Task<string> CompileFormsAsync(string forms) => Task.FromResult(forms);

        public Task<string> CompileExamplesAsync(ObservableCollection<DefinitionViewModel> definitionVMs)
        {
            var selectedExampleVMs = definitionVMs
                .SelectMany(x => x.Examples)
                .Where(x => x.IsChecked)
                .ToList();

            StringBuilder sb = new StringBuilder();
            int count = selectedExampleVMs.Count;
            int i = 1;

            foreach (var exampleVM in selectedExampleVMs)
            {
                if (count > 1)
                {
                    sb.Append($"{i}. ");
                }

                sb.AppendLine(exampleVM.Example);
                i++;
            }

            return Task.FromResult(sb.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }

        #endregion
    }
}
