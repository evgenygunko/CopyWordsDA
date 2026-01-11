// Ignore Spelling: Anki ankidroid

#if ANDROID
using Android.Content.PM;
#endif

namespace CopyWords.Core.Services
{
    public interface IAnkiDroidService
    {
        bool IsAvailable();

        Task<IEnumerable<string>> GetDeckNamesAsync(CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetModelNamesAsync(CancellationToken cancellationToken);
    }

    public class AnkiDroidService : IAnkiDroidService
    {
        private const string AnkiDroidPackage = "com.ichi2.anki";

        public Task<IEnumerable<string>> GetDeckNamesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(["Deck 1", "Deck 2", "Deck 3"]);
        }

        public Task<IEnumerable<string>> GetModelNamesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<string>>(["Model 1", "Model 2", "Model 3", "Model 4"]);
        }

        public bool IsAvailable()
        {
#if ANDROID
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
#else
            return false;
#endif
        }
    }
}
