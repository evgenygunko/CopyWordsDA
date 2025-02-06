using CopyWords.Parsers.Models;

namespace CopyWords.Parsers.Services
{
    public interface ITranslationsService
    {
        Task<WordModel> TranslateAsync(string url, SourceLanguage sourceLanguage, WordModel wordModel);
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

        public async Task<WordModel> TranslateAsync(string url, SourceLanguage sourceLanguage, WordModel wordModel)
        {
            Models.Translations.Input.TranslationInput translationInput = CreateTranslationInputFromWordModel(sourceLanguage, wordModel);

            Models.Translations.Output.TranslationOutput? translationOutput = await _translatorAPIClient.TranslateAsync(url, translationInput);

            if (translationOutput == null)
            {
                return wordModel;
            }

            WordModel wordModelWithTranslations = CreateWordModelFromTranslationOutput(wordModel, translationOutput);
            return wordModelWithTranslations;
        }

        private Models.Translations.Input.TranslationInput CreateTranslationInputFromWordModel(SourceLanguage sourceLanguage, WordModel wordModel)
        {
            Definition firstDefinition = wordModel.Definitions.First();

            var headwordToTranslate = new Models.Translations.Input.Headword(
                Text: wordModel.Word,
                Meaning: firstDefinition.Contexts.First().Meanings.First().Original ?? "",
                PartOfSpeech: firstDefinition.PartOfSpeech,
                Examples: firstDefinition.Contexts.SelectMany(c => c.Meanings.SelectMany(m => m.Examples.Select(e => e.Original))));

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

        private WordModel CreateWordModelFromTranslationOutput(WordModel wordModel, Models.Translations.Output.TranslationOutput translationOutput)
        {
            var definitionsWithTranslations = new List<Definition>();
            int i = 0;

            foreach (var definition in wordModel.Definitions)
            {
                var contextsWithTranslations = new List<Context>();
                foreach (var context in definition.Contexts)
                {
                    var meaningsWithTranslations = new List<Meaning>();
                    foreach (var meaning in context.Meanings)
                    {
                        string? translationRU = translationOutput.Meanings.FirstOrDefault(m => m.id == i)?.MeaningTranslations.FirstOrDefault(mt => mt.Language == LanguageRU)?.Text;

                        meaningsWithTranslations.Add(new Meaning(
                            Original: meaning.Original,
                            Translation: translationRU,
                            AlphabeticalPosition: meaning.AlphabeticalPosition,
                            Tag: meaning.Tag,
                            ImageUrl: meaning.ImageUrl,
                            Examples: meaning.Examples));
                    }

                    contextsWithTranslations.Add(new Context(
                        ContextEN: "",
                        Position: "",
                        Meanings: meaningsWithTranslations));
                }

                definitionsWithTranslations.Add(new Definition(
                    Headword: CreateHeadWordWithTranslations(wordModel.Word, translationOutput),
                    PartOfSpeech: definition.PartOfSpeech,
                    Endings: definition.Endings,
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

        private Headword CreateHeadWordWithTranslations(string headwordOriginal, Models.Translations.Output.TranslationOutput translationOutput)
        {
            IEnumerable<string>? translationVariantsEN = translationOutput.Headword.FirstOrDefault(x => x.Language == LanguageEN)?.HeadwordTranslations;
            string translationsEN = string.Join(", ", translationVariantsEN ?? []);

            IEnumerable<string>? translationVariantsRU = translationOutput.Headword.FirstOrDefault(x => x.Language == LanguageRU)?.HeadwordTranslations;
            string translationsRU = string.Join(", ", translationVariantsRU ?? []);

            return new Headword(
                Original: headwordOriginal,
                English: translationsEN,
                Russian: translationsRU);
        }
    }
}
