// Ignore Spelling: Snackbar

using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Maui.Core;

namespace CopyWords.Core.Services
{
    public interface IConnectivityService
    {
        void Initialize();

        bool TestConnection();

        Task UpdateConnectivitySnackbarAsync(bool hasConnection, CancellationToken token);

        event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;
    }

    public class ConnectivityService : IConnectivityService
    {
        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        private readonly IDeviceInfo _deviceInfo;
        private readonly ISnackbarService _snackbarService;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
        private System.Timers.Timer? _testTimer;
#pragma warning restore CS0649

        private ISnackbar? _snackbarNoConnection;
        private ISnackbar? _snackbarConnectionRestored;

        // These fields are used only for testing purposes
        private bool _alternateState;
        private bool _isSubscribed;

        public ConnectivityService(
            IDeviceInfo deviceInfo,
            ISnackbarService snackbarService)
        {
            _deviceInfo = deviceInfo;
            _snackbarService = snackbarService;
        }

        ~ConnectivityService()
        {
            if (_isSubscribed)
            {
                Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            }
            _testTimer?.Stop();
            _testTimer?.Dispose();
        }

        #region Public Methods

        public void Initialize()
        {
            if (!_isSubscribed)
            {
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
                _isSubscribed = true;
            }

            // Initialize test timer for simulating connectivity changes
            /*_testTimer = new System.Timers.Timer(TimeSpan.FromSeconds(5));
            _testTimer.Elapsed += TestTimer_Elapsed;
            _testTimer.AutoReset = true;
            _testTimer.Start();*/
        }

        public bool TestConnection()
        {
            var accessType = Connectivity.Current.NetworkAccess;
            return accessType == NetworkAccess.Internet;
        }

        public void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.ConstrainedInternet)
            {
                Debug.WriteLine("Internet access is available but is limited.");
            }
            else if (e.NetworkAccess != NetworkAccess.Internet)
            {
                Debug.WriteLine("Internet access has been lost.");
            }

            // Log each active connection
            Debug.Write("Connections active: ");

            foreach (var item in e.ConnectionProfiles)
            {
                switch (item)
                {
                    case ConnectionProfile.Bluetooth:
                        Debug.Write("Bluetooth");
                        break;
                    case ConnectionProfile.Cellular:
                        Debug.Write("Cell");
                        break;
                    case ConnectionProfile.Ethernet:
                        Debug.Write("Ethernet");
                        break;
                    case ConnectionProfile.WiFi:
                        Debug.Write("WiFi");
                        break;
                }
            }

            Debug.WriteLine(string.Empty);

            // Raise the public event
            ConnectivityChanged?.Invoke(this, e);
        }

        public async Task UpdateConnectivitySnackbarAsync(bool hasConnection, CancellationToken token)
        {
            if (_deviceInfo.Platform != DevicePlatform.Android)
            {
                // Currently we show the snackbar only on Android at the bottom of the screen.
                // On WIndows it shows a small popup window closer to the system tray, so no point in showing it there.
                return;
            }

            if (_snackbarNoConnection is null)
            {
                var snackbarOptions = new SnackbarOptions
                {
                    BackgroundColor = Color.FromArgb("#FF6B6B"),
                    TextColor = Colors.White,
                    ActionButtonTextColor = Colors.White,
                    CornerRadius = new CornerRadius(8)
                };

                string text = "No internet connection";
                string actionButtonText = "Dismiss";
                TimeSpan duration = TimeSpan.FromMinutes(3);

                _snackbarNoConnection = _snackbarService.Make(message: text, action: null, actionButtonText, duration, snackbarOptions);
            }

            if (_snackbarConnectionRestored is null)
            {
                var snackbarOptions = new SnackbarOptions
                {
                    BackgroundColor = Color.FromArgb("#4CAF50"),
                    TextColor = Colors.White,
                    ActionButtonTextColor = Colors.White,
                    CornerRadius = new CornerRadius(8)
                };

                string text = "Connection restored";
                string actionButtonText = "Dismiss";
                TimeSpan duration = TimeSpan.FromSeconds(3);

                _snackbarConnectionRestored = _snackbarService.Make(message: text, action: null, actionButtonText, duration, snackbarOptions);
            }

            if (hasConnection)
            {
                Debug.WriteLine("Internet access is available.");

                await _snackbarNoConnection.Dismiss(token);
                await _snackbarConnectionRestored.Show(token);
            }
            else
            {
                Debug.WriteLine("Internet access has been lost.");

                await _snackbarConnectionRestored.Dismiss(token);
                await _snackbarNoConnection.Show(token);
            }
        }

        #endregion

        #region Private Methods

        private void TestTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // Alternate between None and Internet
            var networkAccess = _alternateState ? NetworkAccess.None : NetworkAccess.Internet;
            _alternateState = !_alternateState;

            // Create test event args
            var connectionProfiles = networkAccess == NetworkAccess.Internet
                ? new List<ConnectionProfile> { ConnectionProfile.WiFi }
                : new List<ConnectionProfile>();

            var eventArgs = new ConnectivityChangedEventArgs(networkAccess, connectionProfiles);

            // Trigger the connectivity changed event
            Connectivity_ConnectivityChanged(this, eventArgs);
        }

        #endregion
    }
}
