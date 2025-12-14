// Ignore Spelling: Anki ankiconnect

using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using Newtonsoft.Json;

namespace CopyWords.Core.Services
{
    public interface IAnkiConnectService
    {
        Task<long> AddNoteAsync(AnkiNote note, CancellationToken cancellationToken);
    }

    public class AnkiConnectService : IAnkiConnectService
    {
        private const string DefaultEndpoint = "http://127.0.0.1:8765";
        private readonly HttpClient _httpClient;

        public AnkiConnectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Public Methods

        public async Task<long> AddNoteAsync(AnkiNote note, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(note);
            if (string.IsNullOrWhiteSpace(note.DeckName))
            {
                throw new ArgumentException("Deck name cannot be null or empty.", nameof(note));
            }

            if (string.IsNullOrWhiteSpace(note.ModelName))
            {
                throw new ArgumentException("Model name cannot be null or empty.", nameof(note));
            }

            // Check if AnkiConnect is running first
            await CheckThatAnkiConnectIsRunningAsync(cancellationToken);

            var request = new AddNoteRequest(
                Action: "addNote",
                Version: 6,
                Params: new AddNoteParams(
                    new AddNoteNote(
                        note.DeckName,
                        note.ModelName,
                        BuildFields(note),
                        note.Tags,
                        Options: BuildOptions(note.Options),
                        Audio: BuildMedia(note.Audio),
                        Video: BuildMedia(note.Video),
                        Picture: BuildMedia(note.Picture))));

            string payload = JsonConvert.SerializeObject(request);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await _httpClient.PostAsync(DefaultEndpoint, content, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var addNoteResponse = JsonConvert.DeserializeObject<AddNoteResponse>(responseBody);
            if (addNoteResponse is null)
            {
                throw new InvalidOperationException("AnkiConnect returned an empty response.");
            }

            if (!response.IsSuccessStatusCode || !string.IsNullOrWhiteSpace(addNoteResponse.Error))
            {
                // todo: check the error: if the note already exists, just show it
                string error = !string.IsNullOrWhiteSpace(addNoteResponse.Error)
                    ? addNoteResponse.Error
                    : $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";

                throw new InvalidOperationException($"Failed to add note to Anki: {error}");
            }

            if (addNoteResponse.Result is null)
            {
                throw new InvalidOperationException("AnkiConnect did not return a note id.");
            }

            long noteId = addNoteResponse.Result.Value;

            // Open the note editor in Anki
            var editRequest = new
            {
                action = "guiEditNote",
                version = 6,
                @params = new
                {
                    note = noteId
                }
            };

            string editPayload = JsonConvert.SerializeObject(editRequest);
            using var editContent = new StringContent(editPayload, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(DefaultEndpoint, editContent, cancellationToken);

            return noteId;
        }

        #endregion

        #region Internal Methods

        internal async Task CheckThatAnkiConnectIsRunningAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _httpClient.GetAsync(DefaultEndpoint, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new AnkiConnectNotRunningException(ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                throw new AnkiConnectNotRunningException(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private static AddNoteNoteOptions? BuildOptions(AnkiNoteOptions? options)
        {
            if (options is null)
            {
                return null;
            }

            AddNoteDuplicateScopeOptions? duplicateScopeOptions = null;
            if (options.DuplicateScopeOptions is not null)
            {
                duplicateScopeOptions = new AddNoteDuplicateScopeOptions(
                    options.DuplicateScopeOptions.DeckName,
                    options.DuplicateScopeOptions.CheckChildren,
                    options.DuplicateScopeOptions.CheckAllModels);
            }

            return new AddNoteNoteOptions(
                options.AllowDuplicate,
                options.DuplicateScope,
                duplicateScopeOptions);
        }

        private static IEnumerable<AddNoteMedia>? BuildMedia(IEnumerable<AnkiMedia>? media)
        {
            if (media is null)
            {
                return null;
            }

            return media.Select(m => new AddNoteMedia(
                m.Url,
                m.Filename,
                m.SkipHash,
                m.Fields)).ToList();
        }

        private static Dictionary<string, string> BuildFields(AnkiNote note)
        {
            return new Dictionary<string, string>
            {
                ["Front"] = note.Front,
                ["Back"] = note.Back,
                ["PartOfSpeech"] = note.PartOfSpeech ?? string.Empty,
                ["Forms"] = note.Forms ?? string.Empty,
                ["Example"] = note.Example ?? string.Empty,
                ["Sound"] = note.Sound ?? string.Empty,
            };
        }

        #endregion
    }
}