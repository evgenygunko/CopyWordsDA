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
            var inputDefinitions = new List<Models.Translations.Input.Definition>();

            foreach (var definition in wordModel.Definitions)
            {
                var inputMeanings = new List<Models.Translations.Input.Meaning>();

                Meaning firstMeaning = definition.Contexts.First().Meanings.First();

                var headwordToTranslate = new Models.Translations.Input.Headword(
                    Text: definition.Headword.Original,
                    Meaning: firstMeaning.Original ?? "",
                    PartOfSpeech: definition.PartOfSpeech,
                    Examples: firstMeaning.Examples.Select(e => e.Original));

                foreach (var context in definition.Contexts)
                {
                    foreach (var meaning in context.Meanings)
                    {
                        inputMeanings.Add(new Models.Translations.Input.Meaning(
                            id: inputMeanings.Count + 1,
                            Text: meaning.Original,
                            Examples: meaning.Examples.Select(e => e.Original)));
                    }
                }

                inputDefinitions.Add(new Models.Translations.Input.Definition(
                    id: inputDefinitions.Count + 1,
                    Headword: headwordToTranslate,
                    Meanings: inputMeanings));
            }

            var translationInput = new Models.Translations.Input.TranslationInput(
                Version: "2",
                SourceLanguage: sourceLanguage.ToString(),
                DestinationLanguages: DestinationLanguages,
                Definitions: inputDefinitions);

            return translationInput;
        }

        internal WordModel CreateWordModelFromTranslationOutput(
            bool translateHeadword,
            bool translateMeanings,
            WordModel wordModel,
            Models.Translations.Output.TranslationOutput translationOutput)
        {
            var definitionsWithTranslations = new List<Definition>();
            int definitionIndex = 1;

            foreach (var originalDefinition in wordModel.Definitions)
            {
                Models.Translations.Output.DefinitionTranslations translationDefinition = translationOutput.Definitions.First(d => d.id == definitionIndex);

                var contextsWithTranslations = new List<Context>();
                foreach (var originalContext in originalDefinition.Contexts)
                {
                    var meaningsWithTranslations = new List<Meaning>();
                    int meaningIndex = 1;

                    foreach (var originalMeaning in originalContext.Meanings)
                    {
                        string? translationRU = null;
                        if (translateMeanings)
                        {
                            translationRU = translationDefinition.Meanings.FirstOrDefault(m => m.id == meaningIndex)?.MeaningTranslations.FirstOrDefault(mt => mt.Language == LanguageRU)?.Text;
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

                        meaningIndex++;
                    }

                    contextsWithTranslations.Add(new Context(
                        ContextEN: originalContext.ContextEN,
                        Position: originalContext.Position,
                        Meanings: meaningsWithTranslations));
                }

                definitionsWithTranslations.Add(new Definition(
                    Headword: CreateHeadWordWithTranslations(translateHeadword: translateHeadword, originalDefinition.Headword, translationDefinition),
                    PartOfSpeech: originalDefinition.PartOfSpeech,
                    Endings: originalDefinition.Endings,
                    Contexts: contextsWithTranslations));

                definitionIndex++;
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
            Models.Translations.Output.DefinitionTranslations outputDefinition)
        {
            string? translationsEN;
            string? translationsRU;

            if (translateHeadword)
            {
                IEnumerable<string>? translationVariantsEN = outputDefinition.Headword.FirstOrDefault(x => x.Language == LanguageEN)?.HeadwordTranslations;
                translationsEN = string.Join(", ", translationVariantsEN ?? []);

                IEnumerable<string>? translationVariantsRU = outputDefinition.Headword.FirstOrDefault(x => x.Language == LanguageRU)?.HeadwordTranslations;
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
