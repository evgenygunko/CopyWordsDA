using CopyWords.Core.Constants;
using FluentAssertions;

namespace CopyWords.Core.Tests.Constants
{
    [TestClass]
    public class ThemeColorsTests
    {
        [TestMethod]
        [DataRow(AppTheme.Light, true, "#55585C")]
        [DataRow(AppTheme.Light, false, "#D0D2D3")]
        [DataRow(AppTheme.Dark, true, "#A9ADB1")]
        [DataRow(AppTheme.Dark, false, "#4A4D50")]
        public void GetButtonBackgroundColor_ReturnsGraphiteThemeColor(AppTheme theme, bool isEnabled, string expected)
        {
            ThemeColors.GetButtonBackgroundColor(theme, isEnabled).Should().Be(Color.FromArgb(expected));
        }

        [TestMethod]
        [DataRow(AppTheme.Light, "#FFFFFF")]
        [DataRow(AppTheme.Dark, "#202224")]
        public void GetButtonForegroundColor_ReturnsContrastingThemeColor(AppTheme theme, string expected)
        {
            ThemeColors.GetButtonForegroundColor(theme).Should().Be(Color.FromArgb(expected));
        }
    }
}
