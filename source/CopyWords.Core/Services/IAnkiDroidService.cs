// Ignore Spelling: Anki ankidroid

using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface IAnkiDroidService
    {
        bool IsAvailable();

        bool HasPermission();

        Task RequestPermissionAsync();

        Task<IEnumerable<string>> GetDeckNamesAsync(CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetModelNamesAsync(CancellationToken cancellationToken);

        Task AddNoteAsync(AnkiNote note, CancellationToken cancellationToken);
    }
}
