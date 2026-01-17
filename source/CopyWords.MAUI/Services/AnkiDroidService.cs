// Ignore Spelling: Anki ankidroid

using CopyWords.Core.Services;
using CopyWords.Core.Models;

#if ANDROID

using Android.Content.PM;
using Com.Ichi2.Anki.Api;
using Java.Lang;
using AndroidX.Core.Content;
using Microsoft.Maui.ApplicationModel;

#endif

namespace CopyWords.MAUI.Services
{
#if ANDROID
    public class AnkiDroidService : IAnkiDroidService
    {
        private const string AnkiDroidPackage = "com.ichi2.anki";
        private const string AnkiDroidPermission = "com.ichi2.anki.permission.READ_WRITE_DATABASE";

        public bool HasPermission()
        {
            var context = Android.App.Application.Context;
            return ContextCompat.CheckSelfPermission(context, AnkiDroidPermission) == Android.Content.PM.Permission.Granted;
        }

        public async Task RequestPermissionAsync()
        {
            await Permissions.RequestAsync<AnkiDroidApiPermission>();
        }

        public Task<IEnumerable<string>> GetDeckNamesAsync(CancellationToken cancellationToken)
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            var managedList = new List<string>();

            IDictionary<Long, string>? javaDict = api.DeckList;
            if (javaDict != null)
            {
                foreach (var kvp in javaDict)
                {
                    managedList.Add(kvp.Value);
                }
            }

            return Task.FromResult<IEnumerable<string>>(managedList);
        }

        public Task<IEnumerable<string>> GetModelNamesAsync(CancellationToken cancellationToken)
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            var managedList = new List<string>();

            IDictionary<Long, string>? javaDict = api.ModelList;
            if (javaDict != null)
            {
                foreach (var kvp in javaDict)
                {
                    managedList.Add(kvp.Value);
                }
            }

            return Task.FromResult<IEnumerable<string>>(managedList);
        }

        public bool IsAvailable()
        {
            var ctx = Android.App.Application.Context;

            // Basic presence check
            try
            {
                ctx.PackageManager!.GetPackageInfo(AnkiDroidPackage, PackageInfoFlags.MetaData);
                return true;
            }
            catch (PackageManager.NameNotFoundException)
            {
                return false;
            }
        }

        public Task AddNoteAsync(AnkiNote note, CancellationToken cancellationToken)
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            // Get deck ID from deck name
            long? deckId = GetDeckId(api, note.DeckName);
            if (deckId is null)
            {
                throw new Core.Exceptions.AnkiDroidAPINotAvailableException($"Deck '{note.DeckName}' not found in AnkiDroid.");
            }

            // Get model ID from model name
            long? modelId = GetModelId(api, note.ModelName);
            if (modelId is null)
            {
                throw new Core.Exceptions.AnkiDroidAPINotAvailableException($"Model '{note.ModelName}' not found in AnkiDroid.");
            }

            // Get field names for the model
            string[]? fieldNames = api.GetFieldList(modelId.Value);
            if (fieldNames is null || fieldNames.Length == 0)
            {
                throw new Core.Exceptions.AnkiDroidAPINotAvailableException($"No fields found for model '{note.ModelName}'.");
            }

            // Build fields array matching the model's field order
            string[] fields = BuildFieldsArray(fieldNames, note);

            // Add the note
            var noteUri = api.AddNote(modelId.Value, deckId.Value, fields, null);
            if (noteUri is null)
            {
                throw new InvalidOperationException("Failed to add note to AnkiDroid. The note may be a duplicate.");
            }

            return Task.CompletedTask;
        }

        private static long? GetDeckId(AddContentApi api, string deckName)
        {
            IDictionary<Long, string>? deckList = api.DeckList;
            if (deckList is null)
            {
                return null;
            }

            foreach (var kvp in deckList)
            {
                if (kvp.Value == deckName)
                {
                    return kvp.Key.LongValue();
                }
            }

            return null;
        }

        private static long? GetModelId(AddContentApi api, string modelName)
        {
            IDictionary<Long, string>? modelList = api.ModelList;
            if (modelList is null)
            {
                return null;
            }

            foreach (var kvp in modelList)
            {
                if (kvp.Value == modelName)
                {
                    return kvp.Key.LongValue();
                }
            }

            return null;
        }

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
    }

    /// <summary>
    /// Custom permission class for AnkiDroid API access
    /// </summary>
    public class AnkiDroidApiPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            [("com.ichi2.anki.permission.READ_WRITE_DATABASE", true)];
    }

#else
    public class AnkiDroidService : IAnkiDroidService
    {
        public bool HasPermission() => true;

        public async Task RequestPermissionAsync() => await Task.CompletedTask;

        public Task<IEnumerable<string>> GetDeckNamesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>([]);
        }

        public Task<IEnumerable<string>> GetModelNamesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>([]);
        }

        public bool IsAvailable() => false;

        public async Task AddNoteAsync(AnkiNote note, CancellationToken cancellationToken) => await Task.CompletedTask;
    }
#endif
}
