
namespace CopyWords.Core.Services.Wrappers
{
    public class AppInfoWrapper : IAppInfo
    {
        public string VersionString => AppInfo.VersionString;

        public string PackageName => AppInfo.PackageName;

        public string Name => AppInfo.Name;

        public Version Version => AppInfo.Version;

        public string BuildString => AppInfo.BuildString;

        public AppTheme RequestedTheme => AppInfo.RequestedTheme;

        public AppPackagingModel PackagingModel => AppInfo.PackagingModel;

        public LayoutDirection RequestedLayoutDirection => AppInfo.RequestedLayoutDirection;

        public void ShowSettingsUI() => AppInfo.ShowSettingsUI();
    }
}
