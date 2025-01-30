using System.Globalization;
using System.Text;
using CopyWords.Core.ViewModels;

namespace CopyWords.Core.Services
{
    public interface ICopySelectedToClipboardService
    {
        Task<string> CompileFrontAsync(DefinitionViewModel definitionViewModel);

        Task<string> CompileBackAsync(DefinitionViewModel definitionViewModel);

        Task<string> CompilePartOfSpeechAsync(DefinitionViewModel definitionViewModel);

        Task<string> CompileEndingsAsync(DefinitionViewModel definitionViewModel);

        Task<string> CompileExamplesAsync(DefinitionViewModel definitionViewModel);
    }

    public class CopySelectedToClipboardService : ICopySelectedToClipboardService
    {
        private const string TemplateGrayText = "<span style=\"color: rgba(0, 0, 0, 0.4)\">{0}</span>";

        private readonly ISaveImageFileService _saveImageFileService;

        public CopySelectedToClipboardService(
            ISaveImageFileService saveImageFileService)
        {
            _saveImageFileService = saveImageFileService;
        }

        #region Public Methods

        public Task<string> CompileFrontAsync(DefinitionViewModel definitionViewModel)
        {
            if (definitionViewModel == null)
            {
                return Task.FromResult(string.Empty);
            }

            string word = definitionViewModel.Word;
            string partOfSpeech = definitionViewModel.PartOfSpeech;

            string front = word;

            if (!string.IsNullOrEmpty(word))
            {
                if (!string.IsNullOrEmpty(partOfSpeech))
                {
                    // Danish
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

                    // Spanish
                    if (partOfSpeech.Equals("MASCULINE NOUN", StringComparison.OrdinalIgnoreCase))
                    {
                        front = $"un {word}";
                    }
                    if (partOfSpeech.Equals("FEMININE NOUN", StringComparison.OrdinalIgnoreCase))
                    {
                        front = $"una {word}";
                    }
                    if (partOfSpeech.Equals("MASCULINE OR FEMININE NOUN", StringComparison.OrdinalIgnoreCase))
                    {
                        front = word + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "m/f");
                    }
                }
            }

            return Task.FromResult(front);
        }

        public async Task<string> CompileBackAsync(DefinitionViewModel definitionViewModel)
        {
            if (definitionViewModel == null)
            {
                return string.Empty;
            }

            List<string> backMeanings = new();
            int imageIndex = 0;
            bool translateMeanings = false;

            foreach (var contextVM in definitionViewModel.ContextViewModels)
            {
                bool isContextAddedToFirstBackMeaning = false;

                foreach (var meaningVM in contextVM.MeaningViewModels)
                {
                    // The meaning is the same for all examples - so find first example which is selected
                    ExampleViewModel exampleVM = meaningVM.ExampleViewModels.FirstOrDefault(x => x.IsChecked);
                    if (exampleVM != null)
                    {
                        var backMeaning = new StringBuilder();

                        backMeaning.Append(meaningVM.Original);

                        // We add context only to first translation so that it doesn't clutter view
                        if (!isContextAddedToFirstBackMeaning && !string.IsNullOrEmpty(contextVM.ContextEN))
                        {
                            backMeaning.Append(" ");
                            backMeaning.Append(contextVM.ContextEN);
                        }

                        if (!string.IsNullOrEmpty(meaningVM.Tag))
                        {
                            backMeaning.Insert(0, $"<span style=\"color:#404040; background-color:#eaeff2; border:1px solid #CCCCCC; margin-right:10px; font-size: 80%;\">{meaningVM.Tag}</span>");
                        }

                        // If translation for the meaning exists, add it to the new line
                        if (!string.IsNullOrEmpty(meaningVM.Translation))
                        {
                            backMeaning.Append("<br>");
                            backMeaning.AppendFormat(TemplateGrayText, meaningVM.Translation);
                            translateMeanings |= true;
                        }

                        if (meaningVM.IsImageChecked && !string.IsNullOrEmpty(meaningVM.ImageUrl))
                        {
                            // Download image, resize and save to Anki media collection folder
                            string imageFileName = definitionViewModel.Word;
                            if (imageIndex > 0)
                            {
                                imageFileName += imageIndex;
                            }

                            bool result = await _saveImageFileService.SaveImageFileAsync(meaningVM.ImageUrl, imageFileName);
                            if (result)
                            {
                                backMeaning.Append($"<br><img src=\"{imageFileName}.jpg\">");
                                imageIndex++;
                            }
                        }

                        backMeanings.Add(backMeaning.ToString());
                        isContextAddedToFirstBackMeaning = true;
                    }
                }
            }

            StringBuilder sb = new StringBuilder();

            // If translation for the headword is selected, add it first.
            string format = (backMeanings.Count > 0) ? TemplateGrayText : "{0}";
            var headwordVM = definitionViewModel.HeadwordViewModel;
            if (headwordVM.IsRussianTranslationChecked && !string.IsNullOrEmpty(headwordVM.Russian))
            {
                sb.Append(string.Format(CultureInfo.CurrentCulture, format, headwordVM.Russian) + "<br>");
            }
            if (headwordVM.IsEnglishTranslationChecked && !string.IsNullOrEmpty(headwordVM.English))
            {
                sb.Append(string.Format(CultureInfo.CurrentCulture, format, headwordVM.English) + "<br>");
            }

            // Now go through all meanings and add numbering
            int count = backMeanings.Count;
            int i = 1;

            foreach (string backMeaning in backMeanings)
            {
                if (count > 1)
                {
                    sb.Append($"{i}.&nbsp;");
                }

                // Run some cleanup - e.g. when both translation and context have the same word and we glue them, remove duplicates
                sb.Append(backMeaning.Replace("(colloquial) (colloquial)", "(colloquial)", StringComparison.CurrentCulture));

                if (i < count)
                {
                    // Spanish meanings are usually very short, so a horizontal line doesn't look good.
                    // For Danish dictionary, add <hr> tag only if "translate meanings" is selected.
                    sb.Append(translateMeanings ? "<hr>" : "<br>");
                }

                i++;
            }

            string text = sb.ToString();
            if (text.EndsWith("<br>"))
            {
                text = text.Substring(0, text.LastIndexOf("<br>"));
            }

            return text;
        }

        public Task<string> CompilePartOfSpeechAsync(DefinitionViewModel definitionViewModel) => Task.FromResult(definitionViewModel.PartOfSpeech);

        public Task<string> CompileEndingsAsync(DefinitionViewModel definitionViewModel) => Task.FromResult(definitionViewModel.Endings);

        public Task<string> CompileExamplesAsync(DefinitionViewModel definitionViewModel)
        {
            var selectedExampleVMs = definitionViewModel.ContextViewModels
                .SelectMany(x => x.MeaningViewModels)
                .SelectMany(x => x.ExampleViewModels)
                .Where(x => x.IsChecked)
                .ToList();

            StringBuilder sb = new StringBuilder();
            int count = selectedExampleVMs.Count;
            int i = 1;

            foreach (var exampleVM in selectedExampleVMs)
            {
                if (count > 1)
                {
                    sb.Append(CultureInfo.CurrentCulture, $"<span style=\"color: rgba(0, 0, 0, 1)\">{i}.&nbsp;{exampleVM.Original}</span>");
                }
                else
                {
                    sb.Append(CultureInfo.CurrentCulture, $"<span style=\"color: rgba(0, 0, 0, 1)\">{exampleVM.Original}</span>");
                }

                if (!string.IsNullOrEmpty(exampleVM.Translation))
                {
                    sb.Append("&nbsp;");
                    sb.Append("<span style=\"color: rgba(0, 0, 0, 0.4)\">" + exampleVM.Translation + "</span>");
                }

                if (i < count)
                {
                    sb.Append("<br>");
                }

                i++;
            }

            return Task.FromResult(sb.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }

        #endregion
    }
}
