using CopyWords.Core.Constants;
using CopyWords.Core.Models;
using FluentAssertions;

namespace CopyWords.Core.Tests.Constants
{
    [TestClass]
    public class ThemeColorsTests
    {
        [TestMethod]
        [DataRow(AppColorTheme.Blue, true, "#512BD4")]
        [DataRow(AppColorTheme.Blue, false, "#C8C8C8")]
        [DataRow(AppColorTheme.Graphite, true, "#55585C")]
        [DataRow(AppColorTheme.Graphite, false, "#D0D2D3")]
        [DataRow(AppColorTheme.Dark, true, "#A9ADB1")]
        [DataRow(AppColorTheme.Dark, false, "#4A4D50")]
        public void GetButtonBackgroundColor_ReturnsThemeColor(AppColorTheme theme, bool isEnabled, string expected)
        {
            ThemeColors.GetButtonBackgroundColor(theme, isEnabled).Should().Be(Color.FromArgb(expected));
        }

        [TestMethod]
        [DataRow(AppColorTheme.Blue, "#FFFFFF")]
        [DataRow(AppColorTheme.Graphite, "#FFFFFF")]
        [DataRow(AppColorTheme.Dark, "#202224")]
        public void GetButtonForegroundColor_ReturnsContrastingThemeColor(AppColorTheme theme, string expected)
        {
            ThemeColors.GetButtonForegroundColor(theme).Should().Be(Color.FromArgb(expected));
        }
    }
}
