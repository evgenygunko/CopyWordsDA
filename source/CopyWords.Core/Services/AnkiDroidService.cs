// Ignore Spelling: Anki ankidroid

using CopyWords.Core.Exceptions;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface IAnkiDroidService
    {
        bool IsAvailable();

        bool HasPermission();

        Task RequestPermissionAsync();

        IEnumerable<string> GetDeckNames();

        IEnumerable<string> GetModelNames();

        long AddNote(AnkiNote note);
    }

    public class AnkiDroidService : IAnkiDroidService
    {
        private readonly IAnkiContentApi _ankiContentApi;

        public AnkiDroidService(IAnkiContentApi ankiContentApi)
        {
            _ankiContentApi = ankiContentApi;
        }

        #region Public Methods

        public bool IsAvailable() => _ankiContentApi.IsAvailable();

        public bool HasPermission() => _ankiContentApi.HasPermission();

        public async Task RequestPermissionAsync() => await _ankiContentApi.RequestPermissionAsync();

        public IEnumerable<string> GetDeckNames() =>
            _ankiContentApi.GetDeckList()?.Values.ToList() ?? [];

        public IEnumerable<string> GetModelNames() =>
            _ankiContentApi.GetModelList()?.Values.ToList() ?? [];

        public long AddNote(AnkiNote note)
        {
            // Get deck ID from deck name
            long? deckId = _ankiContentApi.GetDeckList()?
                .FirstOrDefault(kv => kv.Value == note.DeckName).Key;

            // If no deck is found, Key will be 0 (default for long), so check if the deck exists
            if (deckId == null || deckId == 0)
            {
                throw new AnkiDroidDeckNotFoundException($"Deck '{note.DeckName}' not found in AnkiDroid.");
            }

            // Get model ID from model name
            long? modelId = _ankiContentApi.GetModelList()?
                .FirstOrDefault(kv => kv.Value == note.ModelName).Key;

            // If no model is found, Key will be 0 (default for long), so check if the model exists
            if (modelId is null || modelId == 0)
            {
                throw new AnkiDroidModelNotFoundException($"Model '{note.ModelName}' not found in AnkiDroid.");
            }

            // Get field names for the model
            string[]? fieldNames = _ankiContentApi.GetFieldList(modelId.Value);
            if (fieldNames is null || fieldNames.Length == 0)
            {
                throw new AnkiDroidFieldsNotFoundException($"No fields found for model '{note.ModelName}'.");
            }

            // Build fields array matching the model's field order
            string[] fields = BuildFieldsArray(fieldNames, note);

            // Add the note
            long noteId = _ankiContentApi.AddNote(modelId.Value, deckId.Value, fields, null);

            // todo: handle duplicates
            return noteId;
        }

        #endregion

        #region Private Methods

        private static string[] BuildFieldsArray(string[] fieldNames, AnkiNote note)
        {
            var fields = new string[fieldNames.Length];

            for (int i = 0; i < fieldNames.Length; i++)
            {
                fields[i] = fieldNames[i] switch
                {
                    "Front" => note.Front,
                    "Back" => note.Back,
                    "PartOfSpeech" => note.PartOfSpeech ?? string.Empty,
                    "Forms" => note.Forms ?? string.Empty,
                    "Example" => note.Example ?? string.Empty,
                    "Sound" => note.Sound ?? string.Empty,
                    _ => string.Empty
                };
            }

            return fields;
        }

        #endregion
    }
}
