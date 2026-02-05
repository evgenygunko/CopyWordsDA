using System.Text.Json;
using CopyWords.Core.Models;

namespace CopyWords.Core.Services
{
    public interface IUpdateService
    {
        Task<ReleaseInfo> GetLatestReleaseVersionAsync();

        Task<bool> IsUpdateAvailableAsync(string currentVersion);
    }

    public class UpdateService : IUpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly IGlobalSettings _globalSettings;

        public UpdateService(HttpClient httpClient, IGlobalSettings globalSettings)
        {
            _httpClient = httpClient;
            _globalSettings = globalSettings;
        }

        public async Task<ReleaseInfo> GetLatestReleaseVersionAsync()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var response = await _httpClient.GetAsync(_globalSettings.LatestReleaseUrl, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var release = JsonDocument.Parse(jsonResponse);

            string versionTag = release.RootElement.GetProperty("tag_name").GetString() ?? "";
            string latestVersion = versionTag.TrimStart('v');
            string description = release.RootElement.GetProperty("body").GetString() ?? "";

            string downloadUrl = "";
            if (release.RootElement.TryGetProperty("assets", out JsonElement assets) && assets.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    if (asset.TryGetProperty("browser_download_url", out JsonElement urlElement)
                        && asset.TryGetProperty("name", out JsonElement nameElement))
                    {
                        string assetName = nameElement.GetString() ?? "";
                        if (assetName.EndsWith(".msix", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = urlElement.GetString() ?? "";
                            break;
                        }
                    }
                }
            }

            return new ReleaseInfo(latestVersion, description, downloadUrl);
        }

        public async Task<bool> IsUpdateAvailableAsync(string currentVersion)
        {
            if (!Uri.TryCreate(_globalSettings.LatestReleaseUrl, UriKind.Absolute, out _))
            {
                return false;
            }

            Version? current;
            if (!Version.TryParse(currentVersion, out current))
            {
                return false;
            }

            ReleaseInfo releaseInfo = await GetLatestReleaseVersionAsync();
            Version? latest;
            if (!Version.TryParse(releaseInfo.LatestVersion, out latest))
            {
                return false;
            }

            return latest > current;
        }
    }
}
