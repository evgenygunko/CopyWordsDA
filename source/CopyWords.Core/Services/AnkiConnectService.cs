// Ignore Spelling: Anki ankiconnect

using System.Text;
using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;
using CopyWords.Core.Services.Wrappers;
using FluentValidation;
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
        private readonly IValidator<AnkiNote> _ankiNoteValidator;
        private readonly IDialogService _dialogService;

        public AnkiConnectService(HttpClient httpClient, IValidator<AnkiNote> ankiNoteValidator, IDialogService dialogService)
        {
            _httpClient = httpClient;
            _ankiNoteValidator = ankiNoteValidator;
            _dialogService = dialogService;
        }

        #region Public Methods

        public async Task<long> AddNoteAsync(AnkiNote note, CancellationToken cancellationToken)
        {
            var validationResult = await _ankiNoteValidator.ValidateAsync(note, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException(validationResult.ToString());
            }

            // Check if AnkiConnect is running first
            await CheckThatAnkiConnectIsRunningAsync(cancellationToken);

            long noteId = 0;
            try
            {
                // Add the note to Anki via AnkiConnect.
                noteId = await AddNoteWithAnkiConnectAsync(note, cancellationToken);
            }
            catch (AnkiNoteExistsException)
            {
                await _dialogService.DisplayAlertAsync("Cannot add note", $"Cannot add '{note.Front}' because it already exists", "OK");

                // todo: find an existing note with given front field
                // noteId = await FindExistingNoteIdAsync(note.Front, cancellationToken);
                return 0;
            }

            // Cards have been added successfully. Now open the note editor in Anki to allow the user to make further edits.
            // Skip showing the edit window if the note was a duplicate (noteId == 0)
            await ShowAnkiEditNoteWindowAsync(noteId, cancellationToken);

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

        internal async Task<long> AddNoteWithAnkiConnectAsync(AnkiNote note, CancellationToken cancellationToken)
        {
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
                // Check if the error is about duplicate note
                if (!string.IsNullOrWhiteSpace(addNoteResponse.Error) &&
                    addNoteResponse.Error.Contains("cannot create note because it is a duplicate", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AnkiNoteExistsException(addNoteResponse.Error);
                }

                string error = !string.IsNullOrWhiteSpace(addNoteResponse.Error)
                    ? addNoteResponse.Error
                    : $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase})";

                throw new InvalidOperationException($"Failed to add note to Anki: {error}");
            }

            if (addNoteResponse.Result is null)
            {
                throw new InvalidOperationException("AnkiConnect did not return a note id.");
            }

            return addNoteResponse.Result.Value;
        }

        internal async Task ShowAnkiEditNoteWindowAsync(long noteId, CancellationToken cancellationToken)
        {
            // Show the note editor in Anki to allow the user to make further edits.
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