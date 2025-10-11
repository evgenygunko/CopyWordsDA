using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class LastCrashViewModel : ObservableObject
    {
        private readonly IPreferences _preferences;
        private readonly IShellService _shellService;
        private readonly IEmailService _emailService;
        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;

        public LastCrashViewModel(
            IPreferences preferences,
            IShellService shellService,
            IEmailService emailService,
            IClipboardService clipboardService,
            IDialogService dialogService)
        {
            _preferences = preferences;
            _shellService = shellService;
            _emailService = emailService;
            _clipboardService = clipboardService;

            ExceptionName = string.Empty;
            ErrorMessage = string.Empty;
            CrashTime = string.Empty;
            StackTrace = string.Empty;
            _dialogService = dialogService;
        }

        [ObservableProperty]
        public partial string ExceptionName { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string CrashTime { get; set; }

        [ObservableProperty]
        public partial string StackTrace { get; set; }

        [RelayCommand]
        public void GetCrashDumpInfo()
        {
            // This command is called from "Appearing" event handler.
            long lastCrashTime = _preferences.Get("LastCrashTime", 0L);

            CrashTime = DateTimeOffset.FromUnixTimeMilliseconds(lastCrashTime).LocalDateTime.ToString();
            ExceptionName = _preferences.Get("LastCrashException", string.Empty);
            ErrorMessage = _preferences.Get("LastCrashMessage", string.Empty);
            StackTrace = _preferences.Get("LastCrashStackTrace", string.Empty);
        }

        [RelayCommand]
        public async Task SendEmailAsync()
        {
            try
            {
                string subject = "CopyWords Application Crash Report";

                string body = $"""
                    Dear Support Team,

                    The CopyWords application has crashed. Here are the details:

                    Crash Time: {CrashTime}
                    Exception Type: {ExceptionName}
                    Error Message: {ErrorMessage}

                    Stack Trace:
                    {StackTrace}

                    Please investigate this issue.

                    Best regards,
                    CopyWords User
                    """;

                await _emailService.ComposeAsync(subject, body);
            }
            catch (Exception)
            {
                // Handle other exceptions - silently fail as this is a non-critical feature
                // Users can still manually report crashes through other means
                Debug.WriteLine("Failed to open email client.");
            }
        }

        [RelayCommand]
        public async Task CopyErrorMessageAsync()
        {
            await _clipboardService.CopyTextToClipboardAsync(ErrorMessage);
            await _dialogService.DisplayToast("Error message copied to clipboard");
        }

        [RelayCommand]
        public async Task CopyStackTraceAsync()
        {
            await _clipboardService.CopyTextToClipboardAsync(StackTrace);
            await _dialogService.DisplayToast("Stack trace copied to clipboard");
        }

        [RelayCommand]
        public async Task CloseDialogAsync()
        {
            _preferences.Set("LastCrashHandled", true);

            await _shellService.PopModalAsync();
        }
    }
}
