using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Core.Exceptions;
using CopyWords.Core.ViewModels;

namespace CopyWords.Core.Services
{
    public interface ICopySelectedToClipboardService
    {
        Task<string> CompileFrontAsync(ObservableCollection<DefinitionViewModel> definitionViewModels);

        Task<string> CompileBackAsync(ObservableCollection<DefinitionViewModel> definitionViewModels);

        Task<string> CompilePartOfSpeechAsync(ObservableCollection<DefinitionViewModel> definitionViewModels);

        Task<string> CompileEndingsAsync(ObservableCollection<DefinitionViewModel> definitionViewModels);

        Task<string> CompileExamplesAsync(ObservableCollection<DefinitionViewModel> definitionViewModels);

        string CompileHeadword(ObservableCollection<DefinitionViewModel> definitionViewModels);
    }

    public class CopySelectedToClipboardService : ICopySelectedToClipboardService
    {
        private const string TemplateGrayText = "<span style=\"color: rgba(0, 0, 0, 0.4)\">{0}</span>";

        private readonly ISaveImageFileService _saveImageFileService;
        private readonly IDeviceInfo _deviceInfo;
        private readonly ISettingsService _settingsService;

        public CopySelectedToClipboardService(
            ISaveImageFileService saveImageFileService,
            IDeviceInfo deviceInfo,
            ISettingsService settingsService)
        {
            _saveImageFileService = saveImageFileService;
            _deviceInfo = deviceInfo;
            _settingsService = settingsService;
        }

        #region Public Methods

        public Task<string> CompileFrontAsync(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            // find the first definition view model which has any example selected
            DefinitionViewModel? definitionViewModel = FindDefinitionViewModelWithSelectedHeadwordOrExamples(definitionViewModels);
            if (definitionViewModel == null)
            {
                return Task.FromResult(string.Empty);
            }

            string word = definitionViewModel.HeadwordViewModel.Original!;
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

                    // Spanish - replace eventual definite articles with indefinite
                    if (partOfSpeech.Equals("MASCULINE NOUN", StringComparison.OrdinalIgnoreCase))
                    {
                        front = $"un {word.Replace("el ", "")}";
                    }

                    // In Spanish, feminine nouns starting with a stressed “a” take el or un in the singular to avoid the awkward sound
                    if (partOfSpeech.Equals("FEMININE NOUN", StringComparison.OrdinalIgnoreCase)
                        && !(word.StartsWith("el ", StringComparison.OrdinalIgnoreCase) || word.StartsWith("un ", StringComparison.OrdinalIgnoreCase)))
                    {
                        front = $"una {word.Replace("la ", "")}";
                    }

                    if (partOfSpeech.Equals("MASCULINE OR FEMININE NOUN", StringComparison.OrdinalIgnoreCase))
                    {
                        front = word + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "m/f");
                    }
                }
            }

            return Task.FromResult(front);
        }

        public async Task<string> CompileBackAsync(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            // find the first definition view model which has any example selected
            DefinitionViewModel? definitionViewModel = FindDefinitionViewModelWithSelectedHeadwordOrExamples(definitionViewModels);
            if (definitionViewModel == null)
            {
                return string.Empty;
            }

            List<string> backMeanings = new();
            int imageIndex = 0;

            foreach (var contextVM in definitionViewModel.ContextViewModels)
            {
                bool isContextAddedToFirstBackMeaning = false;

                foreach (var meaningVM in contextVM.MeaningViewModels)
                {
                    // The meaning is the same for all examples - so find first example which is selected
                    ExampleViewModel? exampleVM = meaningVM.ExampleViewModels.FirstOrDefault(x => x.IsChecked);
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

                        // If translation for the meaning exists and the checkbox "copy translated meanings" enabled, add it to the new line
                        if (!string.IsNullOrEmpty(meaningVM.Translation) && _settingsService.LoadSettings().CopyTranslatedMeanings)
                        {
                            backMeaning.Append("<br>");
                            backMeaning.AppendFormat(TemplateGrayText, meaningVM.Translation);
                        }

                        if (meaningVM.IsImageChecked && !string.IsNullOrEmpty(meaningVM.ImageUrl))
                        {
                            if (_deviceInfo.Platform == DevicePlatform.Android)
                            {
                                // On Android we cannot save files into Android folders, they are protected. We will add a link to the image instead.
                                backMeaning.Append($"<br><img src=\"{meaningVM.ImageUrl}\">");
                            }
                            else
                            {
                                // On Windows and macOS, download the image, resize it, and save it to the Anki media collection folder.
                                string imageFileName = GetImageFileNameWithoutExtension(definitionViewModel.HeadwordViewModel.Original!);
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
                    sb.Append($"{i++}.&nbsp;");
                }

                // Run some cleanup - e.g. when both translation and context have the same word and we glue them, remove duplicates
                sb.Append(backMeaning.Replace("(colloquial) (colloquial)", "(colloquial)", StringComparison.CurrentCulture));

                sb.Append("<br>");
            }

            string text = sb.ToString();
            if (text.EndsWith("<br>"))
            {
                text = text.Substring(0, text.LastIndexOf("<br>"));
            }

            return text;
        }

        public Task<string> CompilePartOfSpeechAsync(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            // find the first definition view model which has any example selected
            DefinitionViewModel? definitionViewModel = FindDefinitionViewModelWithSelectedHeadwordOrExamples(definitionViewModels);
            if (definitionViewModel == null)
            {
                return Task.FromResult(string.Empty);
            }

            return Task.FromResult(definitionViewModel.PartOfSpeech);
        }

        public Task<string> CompileEndingsAsync(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            // find the first definition view model which has any example selected
            DefinitionViewModel? definitionViewModel = FindDefinitionViewModelWithSelectedHeadwordOrExamples(definitionViewModels);
            if (definitionViewModel == null)
            {
                return Task.FromResult(string.Empty);
            }

            return Task.FromResult(definitionViewModel.Endings);
        }

        public Task<string> CompileExamplesAsync(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            DefinitionViewModel? definitionViewModel = FindDefinitionViewModelWithSelectedHeadwordOrExamples(definitionViewModels);
            if (definitionViewModel == null)
            {
                return Task.FromResult(string.Empty);
            }

            var selectedExampleVMs = definitionViewModel
                .ContextViewModels
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

        public string CompileHeadword(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            DefinitionViewModel? definitionViewModel = definitionViewModels.FirstOrDefault();
            if (definitionViewModel == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(definitionViewModel.HeadwordViewModel.Original!);

            if (!string.IsNullOrEmpty(definitionViewModel.PartOfSpeech))
            {
                sb.Append(" (");
                sb.Append(definitionViewModel.PartOfSpeech);
                sb.Append(")");
            }
            sb.AppendLine();

            var headwordVM = definitionViewModel.HeadwordViewModel;

            // In dictionary mode just add translations without formatting
            if (!string.IsNullOrEmpty(headwordVM.Russian))
            {
                sb.AppendLine(headwordVM.Russian);
            }
            if (!string.IsNullOrEmpty(headwordVM.English))
            {
                sb.AppendLine(headwordVM.English);
            }

            // Trim any trailing new lines
            return sb.ToString()
                .TrimEnd(Environment.NewLine.ToCharArray());
        }

        #endregion

        #region Internal Methods

        internal DefinitionViewModel? FindDefinitionViewModelWithSelectedHeadwordOrExamples(ObservableCollection<DefinitionViewModel> definitionViewModels)
        {
            DefinitionViewModel? definitionViewModel = null;

            // Find the view model that has a headword selected.
            var viewModelsWithSelectedHeadwords = definitionViewModels.Where(x => x.HeadwordViewModel.IsEnglishTranslationChecked || x.HeadwordViewModel.IsRussianTranslationChecked);
            if (viewModelsWithSelectedHeadwords.Count() > 1)
            {
                throw new ExamplesFromSeveralDefinitionsSelectedException("You’ve selected headwords from multiple definitions. Please choose headwords from only one.");
            }

            if (viewModelsWithSelectedHeadwords.Count() == 1)
            {
                definitionViewModel = viewModelsWithSelectedHeadwords.First();

                // We found the only view model which has selected a headword. Now verify that other definitions don't have any examples selected.
                var viewModelsWithSelectedExamplesInOtherDefinitions = definitionViewModels.Where(x => x != definitionViewModel
                    && x.ContextViewModels.Any(y => y.MeaningViewModels.Any(z => z.ExampleViewModels.Any(a => a.IsChecked))));
                if (viewModelsWithSelectedExamplesInOtherDefinitions.Count() > 0)
                {
                    throw new ExamplesFromSeveralDefinitionsSelectedException(
                        "You’ve selected examples from definitions other than the one with the selected headword. Please choose examples and a headword from the same definition.");
                }
            }

            // No headwords is selected, find the definition view model that has any example selected.
            var viewModelsWithSelectedExamples = definitionViewModels.Where(x => x.ContextViewModels.Any(y => y.MeaningViewModels.Any(z => z.ExampleViewModels.Any(a => a.IsChecked))));
            if (viewModelsWithSelectedExamples.Count() == 1)
            {
                definitionViewModel = viewModelsWithSelectedExamples.First();
            }
            else if (viewModelsWithSelectedExamples.Count() > 1)
            {
                throw new ExamplesFromSeveralDefinitionsSelectedException("You’ve selected examples from multiple definitions. Please choose examples from only one.");
            }

            return definitionViewModel;
        }

        internal static string GetImageFileNameWithoutExtension(string headword)
        {
            // Find and return the first match of the pattern "el <word>, la <word>"
            const string pattern = @"el\s+(\w+),\s+la\s+\w+";
            Match match = Regex.Match(headword, pattern, RegexOptions.IgnoreCase);

            // If a match is found, return the captured group
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return headword
                .Replace("la ", "")
                .Replace("el ", "");
        }

        #endregion
    }
}
