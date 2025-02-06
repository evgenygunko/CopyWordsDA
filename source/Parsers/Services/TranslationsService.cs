using CopyWords.Parsers.Models;

namespace CopyWords.Parsers.Services
{
    public interface ITranslationsService
    {
        Task<WordModel> TranslateAsync(Options options, WordModel wordModel);
    }

    public class TranslationsService : ITranslationsService
    {
        private const string LanguageRU = "ru";
        private const string LanguageEN = "en";
        private readonly string[] DestinationLanguages = [LanguageRU, LanguageEN];

        private readonly ITranslatorAPIClient _translatorAPIClient;

        public TranslationsService(
            ITranslatorAPIClient translatorAPIClient)
        {
            _translatorAPIClient = translatorAPIClient;
        }

        public async Task<WordModel> TranslateAsync(Options options, WordModel wordModel)
        {
            Models.Translations.Input.TranslationInput translationInput = CreateTranslationInputFromWordModel(options.SourceLang, wordModel);

            Models.Translations.Output.TranslationOutput? translationOutput = await _translatorAPIClient.TranslateAsync(options.TranslatorApiURL!, translationInput);

            if (translationOutput == null)
            {
                return wordModel;
            }

            WordModel wordModelWithTranslations = CreateWordModelFromTranslationOutput(options.TranslateHeadword, options.TranslateMeanings, wordModel, translationOutput);
            return wordModelWithTranslations;
        }

        internal Models.Translations.Input.TranslationInput CreateTranslationInputFromWordModel(SourceLanguage sourceLanguage, WordModel wordModel)
        {
            Definition firstDefinition = wordModel.Definitions.First();
            Meaning firstMeaning = firstDefinition.Contexts.First().Meanings.First();

            var headwordToTranslate = new Models.Translations.Input.Headword(
                Text: wordModel.Word,
                Meaning: firstMeaning.Original ?? "",
                PartOfSpeech: firstDefinition.PartOfSpeech,
                Examples: firstMeaning.Examples.Select(e => e.Original));

            var meaningsToTranslate = new List<Models.Translations.Input.Meaning>();
            int i = 0;

            foreach (var definition in wordModel.Definitions)
            {
                foreach (var context in definition.Contexts)
                {
                    foreach (var meaning in context.Meanings)
                    {
                        meaningsToTranslate.Add(new Models.Translations.Input.Meaning(
                            id: i++,
                            Text: meaning.Original,
                            Examples: meaning.Examples.Select(e => e.Original)));
                    }
                }
            }

            var translationInput = new Models.Translations.Input.TranslationInput(
                Version: "1",
                SourceLanguage: sourceLanguage.ToString(),
                DestinationLanguages: DestinationLanguages,
                Headword: headwordToTranslate,
                Meanings: meaningsToTranslate);
            return translationInput;
        }

        internal WordModel CreateWordModelFromTranslationOutput(
            bool translateHeadword,
            bool translateMeanings,
            WordModel wordModel,
            Models.Translations.Output.TranslationOutput translationOutput)
        {
            var definitionsWithTranslations = new List<Definition>();
            int i = 0;

            foreach (var originalDefinition in wordModel.Definitions)
            {
                var contextsWithTranslations = new List<Context>();
                foreach (var originalContext in originalDefinition.Contexts)
                {
                    var meaningsWithTranslations = new List<Meaning>();
                    foreach (var originalMeaning in originalContext.Meanings)
                    {
                        string? translationRU = null;
                        if (translateMeanings)
                        {
                            translationRU = translationOutput.Meanings.FirstOrDefault(m => m.id == i)?.MeaningTranslations.FirstOrDefault(mt => mt.Language == LanguageRU)?.Text;
                        }
                        else
                        {
                            translationRU = originalMeaning.Translation;
                        }

                        meaningsWithTranslations.Add(new Meaning(
                            Original: originalMeaning.Original,
                            Translation: translationRU,
                            AlphabeticalPosition: originalMeaning.AlphabeticalPosition,
                            Tag: originalMeaning.Tag,
                            ImageUrl: originalMeaning.ImageUrl,
                            Examples: originalMeaning.Examples));

                        i++;
                    }

                    contextsWithTranslations.Add(new Context(
                        ContextEN: originalContext.ContextEN,
                        Position: originalContext.Position,
                        Meanings: meaningsWithTranslations));
                }

                definitionsWithTranslations.Add(new Definition(
                    Headword: CreateHeadWordWithTranslations(translateHeadword: translateHeadword, originalDefinition.Headword, translationOutput),
                    PartOfSpeech: originalDefinition.PartOfSpeech,
                    Endings: originalDefinition.Endings,
                    Contexts: contextsWithTranslations));
            }

            WordModel wordModelWithTranslations = new WordModel(
                Word: wordModel.Word,
                SoundUrl: wordModel.SoundUrl,
                SoundFileName: wordModel.SoundFileName,
                Definitions: definitionsWithTranslations,
                Variations: wordModel.Variations);
            return wordModelWithTranslations;
        }

        private Headword CreateHeadWordWithTranslations(
            bool translateHeadword,
            Headword headwordOriginal,
            Models.Translations.Output.TranslationOutput translationOutput)
        {
            string? translationsEN;
            string? translationsRU;

            if (translateHeadword)
            {
                IEnumerable<string>? translationVariantsEN = translationOutput.Headword.FirstOrDefault(x => x.Language == LanguageEN)?.HeadwordTranslations;
                translationsEN = string.Join(", ", translationVariantsEN ?? []);

                IEnumerable<string>? translationVariantsRU = translationOutput.Headword.FirstOrDefault(x => x.Language == LanguageRU)?.HeadwordTranslations;
                translationsRU = string.Join(", ", translationVariantsRU ?? []);
            }
            else
            {
                translationsEN = headwordOriginal.English;
                translationsRU = headwordOriginal.Russian;
            }

            return new Headword(
                Original: headwordOriginal.Original,
                English: translationsEN,
                Russian: translationsRU);
        }
    }
}
