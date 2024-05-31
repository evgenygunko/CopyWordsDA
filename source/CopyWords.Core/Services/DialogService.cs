using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CopyWords.Core.Services
{
    public interface IDialogService
    {
        Task DisplayAlert(string title, string message, string cancel);

        Task<bool> DisplayAlert(string title, string message, string accept, string cancel);

        Task DisplayToast(string message);
    }

    public class DialogService : IDialogService
    {
        public async Task DisplayAlert(string title, string message, string cancel)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public async Task DisplayToast(string message)
        {
            var toast = Toast.Make(message, ToastDuration.Short);
            await toast.Show();
        }
    }
}
