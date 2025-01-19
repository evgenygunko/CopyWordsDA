#nullable enable
using System.Text.Json;

namespace CopyWords.Core.Services
{
    public interface IUpdateService
    {
        Task<Version> GetLatestReleaseVersionAsync();

        Task<bool> IsUpdateAvailableAsync();
    }

    public class UpdateService : IUpdateService
    {
        private const string LatestReleaseUrl = "https://api.github.com/repos/evgenygunko/CopyWordsDA/releases/latest";

        private readonly HttpClient _httpClient;

        public UpdateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Version> GetLatestReleaseVersionAsync()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var response = await _httpClient.GetAsync(LatestReleaseUrl);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var release = JsonDocument.Parse(jsonResponse);
            string? versionTag = release.RootElement.GetProperty("tag_name").GetString();

            if (!string.IsNullOrEmpty(versionTag))
            {
                versionTag = versionTag.TrimStart('v');
                if (Version.TryParse(versionTag, out Version? version))
                {
                    return version;
                }
            }

            return new Version(0, 0);
        }

        public Task<bool> IsUpdateAvailableAsync()
        {
            // todo: to implement
            return Task.FromResult(false);
        }
    }
}
