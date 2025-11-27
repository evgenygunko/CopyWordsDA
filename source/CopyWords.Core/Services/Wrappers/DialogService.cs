using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CopyWords.Core.Services.Wrappers
{
    public interface IDialogService
    {
        Task DisplayAlertAsync(string title, string message, string cancel);

        Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel);

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

        Task<string> DisplayActionSheetAsync(
            string title,
            string cancel,
            string destruction,
            FlowDirection flowDirection,
            params string[] buttons);
    }

    public class DialogService : IDialogService
    {
        public async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlertAsync(title, message, cancel);
        }

        public async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            return await Application.Current!.Windows[0].Page!.DisplayAlertAsync(title, message, accept, cancel);
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

        public async Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
        {
            return await Application.Current!.Windows[0].Page!.DisplayActionSheetAsync(title, cancel, destruction, flowDirection, buttons);
        }
    }
}
