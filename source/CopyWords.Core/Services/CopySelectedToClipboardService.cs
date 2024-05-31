using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.ViewModels;

namespace CopyWords.Core.Services
{
    public interface ICopySelectedToClipboardService
    {
        Task<string> CompileFrontAsync(ObservableCollection<WordVariantViewModel> wordVariantViewModels);

        Task<string> CompileBackAsync(ObservableCollection<WordVariantViewModel> wordVariantViewModels);

        Task<string> CompileExamplesAsync(ObservableCollection<WordVariantViewModel> wordVariantViewModels);
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

        public Task<string> CompileFrontAsync(ObservableCollection<WordVariantViewModel> wordVariantViewModels)
        {
            if (wordVariantViewModels == null)
            {
                return Task.FromResult(string.Empty);
            }

            string variant = string.Empty;
            string wordType = string.Empty;

            int selectedVariant = 0;
            int i = 0;

            foreach (var variantVM in wordVariantViewModels)
            {
                foreach (var contextVM in variantVM.ContextViewModels)
                {
                    foreach (var translationVM in contextVM.TranslationViewModels)
                    {
                        foreach (var exampleVM in translationVM.ExampleViewModels)
                        {
                            if (exampleVM.IsChecked)
                            {
                                if (!string.IsNullOrEmpty(variant) && selectedVariant != i)
                                {
                                    throw new PrepareWordForCopyingException("Cannot copy front word, examples from several variants selected");
                                }

                                variant = variantVM.WordES;
                                wordType = variantVM.WordType;
                                selectedVariant = i;
                            }
                        }
                    }
                }

                i++;
            }

            string front = variant;

            if (!string.IsNullOrEmpty(variant))
            {
                if (string.Equals(wordType, "MASCULINE NOUN", StringComparison.OrdinalIgnoreCase))
                {
                    front = $"un {variant}";
                }
                else if (string.Equals(wordType, "FEMININE NOUN", StringComparison.OrdinalIgnoreCase))
                {
                    front = $"una {variant}";
                }
                else if (string.Equals(wordType, "MASCULINE OR FEMININE NOUN", StringComparison.OrdinalIgnoreCase))
                {
                    front = variant + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "m/f");
                }
                else if (string.Equals(wordType, "ADVERB", StringComparison.OrdinalIgnoreCase))
                {
                    front = variant + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "ADVERB");
                }
                else if (string.Equals(wordType, "ADJECTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    front = variant + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "ADJECTIVE");
                }
                else if (string.Equals(wordType, "PHRASE", StringComparison.OrdinalIgnoreCase))
                {
                    front = variant + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "PHRASE");
                }
            }

            return Task.FromResult(front);
        }

        public async Task<string> CompileBackAsync(ObservableCollection<WordVariantViewModel> wordVariantViewModels)
        {
            if (wordVariantViewModels == null)
            {
                return string.Empty;
            }

            List<string> backMeanings = new();

            foreach (var variantVM in wordVariantViewModels)
            {
                foreach (var contextVM in variantVM.ContextViewModels)
                {
                    bool isContextAddedToFirstBackMeaning = false;

                    foreach (var translationVM in contextVM.TranslationViewModels)
                    {
                        // The meaning is the same for all examples - so find first example which is selected
                        ExampleViewModel exampleVM = translationVM.ExampleViewModels.FirstOrDefault(x => x.IsChecked);
                        if (exampleVM != null)
                        {
                            string backMeaning = translationVM.English;

                            // We add context only to first translation so that it doesn't clutter view
                            if (!isContextAddedToFirstBackMeaning)
                            {
                                backMeaning += " " + contextVM.ContextEN;
                            }

                            if (translationVM.IsImageChecked && !string.IsNullOrEmpty(translationVM.ImageUrl))
                            {
                                // Download image, resize and save to Anki media collection folder
                                bool result = await _saveImageFileService.SaveImageFileAsync(translationVM.ImageUrl, variantVM.WordES);
                                if (result)
                                {
                                    backMeaning += $"<br><img src=\"{variantVM.WordES}.jpg\">";
                                }
                            }

                            backMeanings.Add(backMeaning);
                            isContextAddedToFirstBackMeaning = true;
                        }
                    }
                }
            }

            // Now go through all meanings and add numbering
            StringBuilder sb = new StringBuilder();
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
                    sb.Append("<br>");
                }

                i++;
            }

            return sb.ToString();
        }

        public Task<string> CompileExamplesAsync(ObservableCollection<WordVariantViewModel> wordVariantViewModels)
        {
            var selectedExampleVMs = wordVariantViewModels
                .SelectMany(x => x.ContextViewModels)
                .SelectMany(x => x.TranslationViewModels)
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
                    sb.Append(CultureInfo.CurrentCulture, $"<span style=\"color: rgba(0, 0, 0, 1)\">{i}.&nbsp;{exampleVM.ExampleES}</span>");
                }
                else
                {
                    sb.Append(CultureInfo.CurrentCulture, $"<span style=\"color: rgba(0, 0, 0, 1)\">{exampleVM.ExampleES}</span>");
                }
                sb.Append("&nbsp;");
                sb.Append("<span style=\"color: rgba(0, 0, 0, 0.4)\">" + exampleVM.ExampleEN + "</span>");

                if (i < count)
                {
                    sb.Append("<br>");
                }

                i++;
            }

            return Task.FromResult(sb.ToString());
        }

        #endregion
    }
}
