using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CopyWords.Core.Services
{
    public interface IDialogService
    {
        Task DisplayAlert(string title, string message, string cancel);

        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);

        Task DisplayToast(string message);

        Task<string> DisplayPromptAsync(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Cancel",
            string? placeholder = default,
            int maxLength = -1,
            Keyboard? keyboard = default,
            string initialValue = "");
    }

    public class DialogService : IDialogService
    {
        public async Task DisplayAlert(string title, string message, string cancel)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert(title, message, cancel);
        }

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await Application.Current!.Windows[0].Page!.DisplayAlert(title, message, accept, cancel);
        }

        public async Task DisplayToast(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Short);
            await toast.Show();
        }

        public async Task<string> DisplayPromptAsync(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Cancel",
            string? placeholder = default,
            int maxLength = -1,
            Keyboard? keyboard = default,
            string initialValue = "")
        {
            return await Application.Current!.Windows[0].Page!.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);
        }
    }
}
