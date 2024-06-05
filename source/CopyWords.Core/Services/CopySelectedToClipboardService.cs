﻿using System.Collections.ObjectModel;
using System.Text;
using CopyWords.Core.ViewModels;

namespace CopyWords.Core.Services
{
    public interface ICopySelectedToClipboardService
    {
        Task<string> CompileFrontAsync(string meaning);

        Task<string> CompileBackAsync(ObservableCollection<DefinitionViewModel> definitionVMs);

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

        public Task<string> CompileFrontAsync(string meaning) => Task.FromResult(meaning);

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
                    meanings.Add(definitionVM.Meaning);
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
                    sb.Append($"{i}. ");
                }

                sb.Append(meaning);

                if (i < count)
                {
                    sb.Append(Environment.NewLine);
                }

                i++;
            }

            return Task.FromResult(sb.ToString());
        }

        public Task<string> CompileExamplesAsync(ObservableCollection<DefinitionViewModel> definitionVMs)
        {
            var selectedExampleVMs = definitionVMs
                .SelectMany(x => x.Examples)
                .Where(x => x.IsChecked)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var exampleVM in selectedExampleVMs)
            {
                sb.AppendLine(exampleVM.Example);
            }

            return Task.FromResult(sb.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }

        #endregion
    }
}
