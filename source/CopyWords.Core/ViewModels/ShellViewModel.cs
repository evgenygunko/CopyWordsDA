// Ignore Spelling: Nav

namespace CopyWords.Core.ViewModels
{
    public class ShellViewModel
    {
        public bool ShowNavBar
        {
            get
            {
                // For some reason RuntimeInformation.IsOSPlatform(OSPlatform.OSX) returns false on macOS. Looks like a bug in MAUI.
                bool isDesktop = OperatingSystem.IsWindows() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS();

                return !isDesktop;
            }
        }
    }
}