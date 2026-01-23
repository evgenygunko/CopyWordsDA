using CopyWords.Core.Services;
using CopyWords.Core.Exceptions;

#if ANDROID

using Android.Content.PM;
using Com.Ichi2.Anki.Api;
using Java.Lang;
using AndroidX.Core.Content;

namespace CopyWords.MAUI.Services
{
    public class AnkiContentApiWrapper : IAnkiContentApi
    {
        private const string AnkiDroidPackage = "com.ichi2.anki";
        private const string AnkiDroidPermission = "com.ichi2.anki.permission.READ_WRITE_DATABASE";

        #region Public Methods

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

        public bool HasPermission()
        {
            var context = Android.App.Application.Context;
            return ContextCompat.CheckSelfPermission(context, AnkiDroidPermission) == Android.Content.PM.Permission.Granted;
        }

        public async Task RequestPermissionAsync()
        {
            await Permissions.RequestAsync<AnkiDroidApiPermission>();
        }

        public IDictionary<long, string>? GetDeckList()
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            IDictionary<Long, string>? javaDict = api.DeckList;
            return ConvertToDotNetDictionary(javaDict);
        }

        public IDictionary<long, string>? GetModelList()
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            IDictionary<Long, string>? javaDict = api.ModelList;
            return ConvertToDotNetDictionary(javaDict);
        }

        public string[]? GetFieldList(long modelId)
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            return api.GetFieldList(modelId);
        }

        public long AddNote(long modelId, long deckId, string[] fields, string[]? tags)
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            Long? noteId = api.AddNote(modelId, deckId, fields, tags);
            return noteId?.LongValue() ?? 0;
        }

        public List<long> FindDuplicateNotes(long modelId, string key)
        {
            var context = Android.App.Application.Context;
            var api = new AddContentApi(context);

            var duplicates = api.FindDuplicateNotes(modelId, key);
            if (duplicates == null)
            {
                return [];
            }

            var result = new List<long>();
            foreach (var noteInfo in duplicates)
            {
                result.Add(noteInfo.Id);
            }

            return result;
        }

        public async Task<string> AddImageToAnkiMediaAsync(string fileName, Stream imageStream)
        {
            var context = Android.App.Application.Context;
            if (context.CacheDir is null)
            {
                throw new AnkiDroidCannotSaveMediaException($"Cannot save media file '{fileName}', CacheDir is null");
            }

            string cachedFilePath = Path.Combine(context.CacheDir.AbsolutePath, fileName);
            Android.Net.Uri? fileUri = null;

            try
            {
                // Save the image stream to a temp file
                using (var fileStream = new FileStream(cachedFilePath, FileMode.Create, FileAccess.Write))
                {
                    await imageStream.CopyToAsync(fileStream);
                }

                // Get a FileProvider URI for the temp file
                var file = new Java.IO.File(cachedFilePath);
                string authority = $"{context.PackageName}.fileprovider";
                fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(context, authority, file);

                if (fileUri == null)
                {
                    throw new AnkiDroidCannotSaveMediaException($"FileProvider returned null URI for file '{fileName}'");
                }

                // Grant URI permission to AnkiDroid
                context.GrantUriPermission(AnkiDroidPackage, fileUri, Android.Content.ActivityFlags.GrantReadUriPermission);

                // Add the media to AnkiDroid
                var api = new AddContentApi(context);
                string? result = api.AddMediaFromUri(fileUri, fileName, "image");

                if (result == null)
                {
                    throw new AnkiDroidCannotSaveMediaException($"AnkiDroid API returned null when adding media file '{fileName}'");
                }

                return result;
            }
            finally
            {
                // Revoke URI permission
                if (fileUri != null)
                {
                    context.RevokeUriPermission(fileUri, Android.Content.ActivityFlags.GrantReadUriPermission);
                }

                // Delete temp file
                if (File.Exists(cachedFilePath))
                {
                    File.Delete(cachedFilePath);
                }
            }
        }

        #endregion

        #region Private Methods

        private static Dictionary<long, string>? ConvertToDotNetDictionary(IDictionary<Long, string>? javaDict)
        {
            if (javaDict is null)
            {
                return null;
            }

            var result = new Dictionary<long, string>();
            foreach (var kvp in javaDict)
            {
                result[kvp.Key.LongValue()] = kvp.Value;
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Custom permission class for AnkiDroid API access
    /// </summary>
    public class AnkiDroidApiPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            [("com.ichi2.anki.permission.READ_WRITE_DATABASE", true)];
    }
}

#else

namespace CopyWords.MAUI.Services;

/// <summary>
/// Stub implementation for non-Android platforms.
/// </summary>
public class AnkiContentApiWrapper : IAnkiContentApi
{
    public bool IsAvailable() => false;
    public bool HasPermission() => false;
    public async Task RequestPermissionAsync() => await Task.CompletedTask;
    public IDictionary<long, string>? GetDeckList() => null;
    public IDictionary<long, string>? GetModelList() => null;
    public string[]? GetFieldList(long modelId) => null;
    public long AddNote(long modelId, long deckId, string[] fields, string[]? tags) => 0;
    public List<long> FindDuplicateNotes(long modelId, string key) => [];
    public async Task<string> AddImageToAnkiMediaAsync(string fileName, Stream imageStream) => await Task.FromResult(string.Empty);
}

#endif