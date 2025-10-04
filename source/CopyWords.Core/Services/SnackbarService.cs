// Ignore Spelling: Snackbar

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CopyWords.Core.Services
{
    public interface ISnackbarService
    {
        ISnackbar Make(string message, Action? action = null, string actionButtonText = "OK", TimeSpan? duration = null, SnackbarOptions? visualOptions = null);
    }

    public class SnackbarService : ISnackbarService
    {
        public ISnackbar Make(string message, Action? action = null, string actionButtonText = "OK", TimeSpan? duration = null, SnackbarOptions? visualOptions = null)
        {
            return Snackbar.Make(message, action, actionButtonText, duration, visualOptions);
        }
    }
}
