using System.Diagnostics;
using CopyWords.Core.Exceptions;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Client;
using LaunchDarkly.Sdk.Client.Interfaces;

namespace CopyWords.Core.Services
{
    public interface ILaunchDarklyService
    {
        Task InitializeAsync(string contextKey, string mobileKey, string memberId);

        bool GetBooleanFlag(string flagKey, bool defaultValue = false);

        bool IsInitialized { get; }
    }

    public class LaunchDarklyService : ILaunchDarklyService, IDisposable
    {
        private ILdClient? _client;
        private bool _disposed;

        public bool IsInitialized => _client?.Initialized ?? false;

        public async Task InitializeAsync(string contextKey, string mobileKey, string memberId)
        {
            if (string.IsNullOrWhiteSpace(mobileKey))
            {
                throw new ArgumentException("Mobile key cannot be null or empty.", nameof(mobileKey));
            }

            try
            {
                Context context = Context.New(contextKey);
                _client = await LdClient.InitAsync(mobileKey, ConfigurationBuilder.AutoEnvAttributes.Enabled, context, TimeSpan.FromSeconds(5));

                if (!_client.Initialized)
                {
                    throw new LaunchDarklyInitializationException("LaunchDarkly client could not be initialized within the timeout period.");
                }

                // Tracking memberId tell LaunchDarkly update information in console that the client is connected.
                _client.Track(memberId);
            }
            catch (Exception ex)
            {
                throw new LaunchDarklyInitializationException("An error occurred while initializing LaunchDarkly.", ex);
            }
        }

        public bool GetBooleanFlag(string flagKey, bool defaultValue = false)
        {
            if (_client == null || !_client.Initialized)
            {
                // Log the error but continue with default value
                Debug.WriteLine($"LaunchDarkly client must be initialized before getting flags. Returning default value '{defaultValue}'.");
                return defaultValue;
            }

            try
            {
                return _client.BoolVariation(flagKey, defaultValue);
            }
            catch (Exception ex)
            {
                throw new LaunchDarklyGetFlagException($"Failed to get boolean flag '{flagKey}': {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client?.Dispose();
                    _client = null;
                }

                _disposed = true;
            }
        }
    }
}
