// Ignore Spelling: Anki ankidroid

using CopyWords.Core.Services;

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
        public bool HasPermission()
        {
            return true;
        }

        public async Task RequestPermissionAsync()
        {
            await Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetDeckNamesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>([]);
        }

        public Task<IEnumerable<string>> GetModelNamesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>([]);
        }

        public bool IsAvailable()
        {
            return false;
        }
    }
#endif
}
