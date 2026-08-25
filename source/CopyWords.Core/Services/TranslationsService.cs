using System.Net.Http.Json;
using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;
using Newtonsoft.Json;

namespace CopyWords.Core.Services
{
    public interface ITranslationsService
    {
        Task<WordModel?> LookUpWordAsync(string wordToLookUp, CancellationToken cancellationToken);

        Task<SuggestedWordsModel> GetSuggestedWordsAsync(string wordToLookUp, CancellationToken cancellationToken);
    }

    public class TranslationsService : ITranslationsService
    {
        private readonly HttpClient _httpClient;
        private readonly IGlobalSettings _globalSettings;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchDarklyService? _launchDarklyService;

        public TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings,
            ISettingsService settingsService,
            ILaunchDarklyService launchDarklyService)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
            _launchDarklyService = launchDarklyService;
        }

        internal TranslationsService(
            HttpClient httpClient,
            IGlobalSettings globalSettings,
            ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
            _settingsService = settingsService;
        }

        public string CreateLookUpWordUrl()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v2/Translation/LookUpWord?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public string CreateLookUpWordV3Url()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v3/Translation/LookUpWord?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public string CreateSuggestedWordsUrl()
        {
            return $"{_globalSettings.TranslatorAppUrl.TrimEnd('/')}/api/v2/Translation/SuggestedWords?code={_globalSettings.TranslatorAppRequestCode}";
        }

        public async Task<WordModel?> LookUpWordAsync(string wordToLookUp, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            if (string.IsNullOrEmpty(_globalSettings.TranslatorAppUrl))
            {
                throw new ArgumentException("TranslatorApp URL cannot be null or empty");
            }

            string sourceLanguage = _settingsService.GetSelectedParser();

            if (_launchDarklyService?.GetBooleanFlag("client-side-parsing", false) == true)
            {
                WordModel wordModel = sourceLanguage == nameof(SourceLanguage.Spanish)
                    ? CreateCocheWordModel()
                    : CreateHajWordModel();

                return await TranslateAsync(CreateLookUpWordV3Url(), wordModel, cancellationToken);
            }

            string lookupUrl = CreateLookUpWordUrl();
            string destinationLanguage = _settingsService.GetDestinationLanguage();
            IReadOnlyList<string> activeDictionaries = _settingsService.GetActiveDictionaries();
            if (string.IsNullOrWhiteSpace(destinationLanguage))
            {
                destinationLanguage = "Russian";
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: destinationLanguage,
                ActiveDictionaries: activeDictionaries,
                Version: "2");

            return await TranslateAsync(lookupUrl, input, cancellationToken);
        }

        public async Task<SuggestedWordsModel> GetSuggestedWordsAsync(string wordToLookUp, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(wordToLookUp))
            {
                throw new ArgumentException("Word to look up cannot be null or empty.", nameof(wordToLookUp));
            }

            if (string.IsNullOrEmpty(_globalSettings.TranslatorAppUrl))
            {
                throw new ArgumentException("TranslatorApp URL cannot be null or empty");
            }

            string suggestedWordsUrl = CreateSuggestedWordsUrl();
            string sourceLanguage = _settingsService.GetSelectedParser();
            string destinationLanguage = _settingsService.GetDestinationLanguage();
            IReadOnlyList<string> activeDictionaries = _settingsService.GetActiveDictionaries();
            if (string.IsNullOrWhiteSpace(destinationLanguage))
            {
                destinationLanguage = "Russian";
            }

            var input = new LookUpWordRequest(
                Text: wordToLookUp,
                SourceLanguage: sourceLanguage,
                DestinationLanguage: destinationLanguage,
                ActiveDictionaries: activeDictionaries,
                Version: "2");

            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // The Translator app may use OpenAI API for request processing, so it can take time to return result.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            using HttpResponseMessage response = await _httpClient.PostAsync(suggestedWordsUrl, content, combinedCts.Token);

            if (response.IsSuccessStatusCode)
            {
                SuggestedWordsModel? suggestedWords = await response.Content.ReadFromJsonAsync<SuggestedWordsModel>(combinedCts.Token);
                return suggestedWords ?? new SuggestedWordsModel([]);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                throw new InvalidInputException(errorContent);
            }

            if (response.StatusCode is System.Net.HttpStatusCode.InternalServerError
                or System.Net.HttpStatusCode.BadGateway
                or System.Net.HttpStatusCode.ServiceUnavailable
                or System.Net.HttpStatusCode.GatewayTimeout)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    throw new ServerErrorException(errorContent);
                }
            }

            throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
        }

        internal async Task<WordModel?> TranslateAsync(string url, LookUpWordRequest input, CancellationToken cancellationToken)
        {
            return await TranslateAsync(url, input, input.Text, cancellationToken);
        }

        internal async Task<WordModel?> TranslateAsync(string url, WordModel input, CancellationToken cancellationToken)
        {
            return await TranslateAsync(url, input, input.Word, cancellationToken);
        }

        private async Task<WordModel?> TranslateAsync(string url, object input, string requestedWord, CancellationToken cancellationToken)
        {
            string jsonRequest = JsonConvert.SerializeObject(input);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // The Translator app is calling OpenAI API, so it can take time to return result.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            using HttpResponseMessage response = await _httpClient.PostAsync(url, content, combinedCts.Token);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WordModel>(combinedCts.Token);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                throw new InvalidInputException(errorContent);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new WordNotFoundException(requestedWord);
            }

            if (response.StatusCode is System.Net.HttpStatusCode.InternalServerError
                or System.Net.HttpStatusCode.BadGateway
                or System.Net.HttpStatusCode.ServiceUnavailable
                or System.Net.HttpStatusCode.GatewayTimeout)
            {
                string errorContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    throw new ServerErrorException(errorContent);
                }
            }

            throw new ServerErrorException($"The server returned the error '{response.StatusCode}'.");
        }

        private static WordModel CreateHajWordModel()
        {
            var definition = new Definition(
                new Headword("haj", null, null),
                PartOfSpeech: "substantiv, fælleskøn",
                Endings: "-en, -er, -erne",
                Contexts:
                [
                    new Context("", "",
                    [
                        new Meaning("stor, langstrakt bruskfisk", null, "1", null, null, null,
                        [new Example("Hubertus [vidste], at det var en haj, der kredsede rundt og håbede på, at en sørøver skulle gå planken ud eller blive kølhalet, så den kunne æde ham", null)]),
                        new Meaning("grisk, skrupelløs person der ved ulovlige eller ufine metoder opnår økonomisk gevinst på andres bekostning", null, "2", "SLANG", null, null,
                        [new Example("-", null)]),
                        new Meaning("person der er særlig dygtig til et spil, håndværk el.lign.", null, "3", "SLANG", null, null,
                        [new Example("Chamonix er et \"must\" for dig, som er en haj på ski. Her finder du noget af alpernes \"tuffeste\" skiløb", null)])
                    ])
                ]);

            return new WordModel("haj", SourceLanguage.Danish, null, null, definition, [], []);
        }

        private static WordModel CreateCocheWordModel()
        {
            var definition = new Definition(
                new Headword("el coche", null, null),
                PartOfSpeech: "MASCULINE NOUN",
                Endings: "",
                Contexts:
                [
                    new Context("(vehicle)", "1",
                    [
                        new Meaning("car", null, "a", null, null, null,
                        [new Example("Mi coche no prende porque tiene una falla en el motor.", "My car won't start because of a problem with the engine.")]),
                        new Meaning("automobile", null, "b", null, null, null,
                        [new Example("Todos estos coches tienen bolsas de aire.", "All these automobiles have airbags.")])
                    ]),
                    new Context("(vehicle led by horses)", "2",
                    [
                        new Meaning("carriage", null, "a", null, null, null,
                        [new Example("Los monarcas llegaron en un coche elegante.", "The monarchs arrived in an elegant carriage.")]),
                        new Meaning("coach", null, "b", null, null, null,
                        [new Example("Los coches de caballos se utilizaban mucho más antes de que se inventara el automóvil.", "Horse-drawn coaches were used much more before the invention of the automobile.")])
                    ])
                ]);

            return new WordModel("coche", SourceLanguage.Spanish, null, null, definition, [], []);
        }
    }
}
