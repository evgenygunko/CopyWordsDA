namespace CopyWords.Core.Constants
{
    /// <summary>
    /// Defines color constants used across the application for theming.
    /// These colors are used for button icons and text that need to contrast
    /// with themed button backgrounds.
    /// </summary>
    public static class ThemeColors
    {
        /// <summary>
        /// Color for button icons/text in light theme (white on graphite background).
        /// </summary>
        public static readonly Color BlueThemeButtonForeground = Colors.White;
        public static readonly Color GraphiteThemeButtonForeground = Colors.White;

        /// <summary>
        /// Color for button icons/text in dark theme (dark on lighter graphite background).
        /// </summary>
        public static readonly Color DarkThemeButtonForeground = Color.FromArgb("#202224");

        /// <summary>
        /// Primary button background color in the light theme when enabled.
        /// </summary>
        public static readonly Color BlueThemeButtonEnabledBackground = Color.FromArgb("#512BD4");
        public static readonly Color GraphiteThemeButtonEnabledBackground = Color.FromArgb("#55585C");

        /// <summary>
        /// Primary button background color in the dark theme when enabled.
        /// </summary>
        public static readonly Color DarkThemeButtonEnabledBackground = Color.FromArgb("#A9ADB1");

        /// <summary>
        /// Button background color (gray) when disabled.
        /// </summary>
        public static readonly Color BlueThemeButtonDisabledBackground = Color.FromArgb("#C8C8C8");
        public static readonly Color GraphiteThemeButtonDisabledBackground = Color.FromArgb("#D0D2D3");

        /// <summary>
        /// Button background color in the dark theme when disabled.
        /// </summary>
        public static readonly Color DarkThemeButtonDisabledBackground = Color.FromArgb("#4A4D50");

        /// <summary>
        /// Gets the appropriate button foreground color based on the current theme.
        /// </summary>
        /// <param name="theme">The current application theme.</param>
        /// <returns>The color to use for button icons and text.</returns>
        public static Color GetButtonForegroundColor(global::CopyWords.Core.Models.AppColorTheme theme) => theme switch
        {
            global::CopyWords.Core.Models.AppColorTheme.Dark => DarkThemeButtonForeground,
            global::CopyWords.Core.Models.AppColorTheme.Graphite => GraphiteThemeButtonForeground,
            _ => BlueThemeButtonForeground
        };

        /// <summary>
        /// Gets the appropriate button background color based on enabled state.
        /// </summary>
        /// <param name="theme">The current application theme.</param>
        /// <param name="isEnabled">Whether the button is enabled.</param>
        /// <returns>The color to use for the button background.</returns>
        public static Color GetButtonBackgroundColor(global::CopyWords.Core.Models.AppColorTheme theme, bool isEnabled) => theme switch
        {
            global::CopyWords.Core.Models.AppColorTheme.Dark => isEnabled ? DarkThemeButtonEnabledBackground : DarkThemeButtonDisabledBackground,
            global::CopyWords.Core.Models.AppColorTheme.Graphite => isEnabled ? GraphiteThemeButtonEnabledBackground : GraphiteThemeButtonDisabledBackground,
            _ => isEnabled ? BlueThemeButtonEnabledBackground : BlueThemeButtonDisabledBackground
        };
    }
}
