using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CopyWords.Core.ViewModels;

namespace CopyWords.Core.Services
{
    public interface ICopySelectedToClipboardService
    {
        string CompileFront(DefinitionViewModel definitionViewModel);

        string CompileBack(DefinitionViewModel definitionViewModel);

        string CompilePartOfSpeech(DefinitionViewModel definitionViewModel);

        string CompileEndings(DefinitionViewModel definitionViewModel);

        string CompileExamples(DefinitionViewModel definitionViewModel);

        string CompileTranslations(DefinitionViewModel definitionViewModel);

        string CompileSoundFileName(string soundFileName);

        Task SaveImagesAsync(DefinitionViewModel definitionViewModel);
    }

    public class CopySelectedToClipboardService : ICopySelectedToClipboardService
    {
        private const string TemplateGrayText = "<span style=\"color: rgba(0, 0, 0, 0.4)\">{0}</span>";

        private readonly ISaveImageFileService _saveImageFileService;
        private readonly ISettingsService _settingsService;

        public CopySelectedToClipboardService(
            ISaveImageFileService saveImageFileService,
            ISettingsService settingsService)
        {
            _saveImageFileService = saveImageFileService;
            _settingsService = settingsService;
        }

        #region Public Methods

        public string CompileFront(DefinitionViewModel definitionViewModel)
        {
            string word = definitionViewModel.HeadwordViewModel.Original!;

            string front = word;

            if (!string.IsNullOrEmpty(word))
            {
                string partOfSpeech = definitionViewModel.PartOfSpeech;
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
                        // Check for pattern "el <word>, la <word>"
                        const string pattern = @"^el\s+(\w+),\s+la\s+(\w+)$";
                        Match match = Regex.Match(word, pattern, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            // Replace "el" with "un" and "la" with "una"
                            front = $"un {match.Groups[1].Value}, una {match.Groups[2].Value}";
                        }
                        else
                        {
                            front = word + " " + string.Format(CultureInfo.CurrentCulture, TemplateGrayText, "m/f");
                        }
                    }
                }
            }

            return front;
        }

        public string CompileBack(DefinitionViewModel definitionViewModel)
        {
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
                            // On Windows and macOS, generate the image file name reference. The actual saving is done by SaveImagesAsync.
                            string imageFileName = GetImageFileNameWithoutExtension(definitionViewModel.HeadwordViewModel.Original!);
                            if (imageIndex > 0)
                            {
                                imageFileName += imageIndex;
                            }

                            backMeaning.Append($"<br><img src=\"{imageFileName}.jpg\">");
                            imageIndex++;
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

        public string CompilePartOfSpeech(DefinitionViewModel definitionViewModel) => definitionViewModel.PartOfSpeech;

        public string CompileEndings(DefinitionViewModel definitionViewModel) => definitionViewModel.Endings;

        public string CompileExamples(DefinitionViewModel definitionViewModel)
        {
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

            return sb.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        public string CompileTranslations(DefinitionViewModel definitionViewModel)
        {
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

        public async Task SaveImagesAsync(DefinitionViewModel definitionViewModel)
        {
            int imageIndex = 0;

            foreach (var contextVM in definitionViewModel.ContextViewModels)
            {
                foreach (var meaningVM in contextVM.MeaningViewModels)
                {
                    // The meaning is the same for all examples - so find first example which is selected
                    ExampleViewModel? exampleVM = meaningVM.ExampleViewModels.FirstOrDefault(x => x.IsChecked);
                    if (exampleVM != null && meaningVM.IsImageChecked && !string.IsNullOrEmpty(meaningVM.ImageUrl))
                    {
                        // On Windows and macOS, download the image, resize it, and save it to the Anki media collection folder.
                        string imageFileName = GetImageFileNameWithoutExtension(definitionViewModel.HeadwordViewModel.Original!);
                        if (imageIndex > 0)
                        {
                            imageFileName += imageIndex;
                        }

                        await _saveImageFileService.SaveImageFileAsync(meaningVM.ImageUrl, imageFileName);
                        imageIndex++;
                    }
                }
            }
        }

        public string CompileSoundFileName(string soundFileName) => $"[sound:{soundFileName}.mp3]";

        #endregion

        #region Internal Methods

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
