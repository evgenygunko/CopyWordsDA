using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CopyWords.Core.Services;

namespace CopyWords.Core.ViewModels
{
    public partial class LastCrashViewModel : ObservableObject
    {
        private readonly IPreferences _preferences;
        private readonly IShellService _shellService;

        public LastCrashViewModel(
            IPreferences preferences,
            IShellService shellService)
        {
            _preferences = preferences;
            _shellService = shellService;

            ExceptionName = string.Empty;
            ErrorMessage = string.Empty;
            CrashTime = string.Empty;
            StackTrace = string.Empty;
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
        public async Task CloseDialogAsync()
        {
            _preferences.Set("LastCrashHandled", true);

            await _shellService.PopModalAsync();
        }
    }
}
