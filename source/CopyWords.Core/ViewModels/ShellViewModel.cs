// Ignore Spelling: Nav

using System.Runtime.InteropServices;

namespace CopyWords.Core.ViewModels
{
    public class ShellViewModel
    {
        public bool ShowNavBar => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}