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
        /// Color for button icons/text in light theme (white on purple background).
        /// </summary>
        public static readonly Color LightThemeButtonForeground = Colors.White;

        /// <summary>
        /// Color for button icons/text in dark theme (dark on lighter purple background).
        /// </summary>
        public static readonly Color DarkThemeButtonForeground = Color.FromArgb("#121212");

        /// <summary>
        /// Primary button background color (purple) when enabled.
        /// </summary>
        public static readonly Color ButtonEnabledBackground = Color.FromArgb("#512BD4");

        /// <summary>
        /// Button background color (gray) when disabled.
        /// </summary>
        public static readonly Color ButtonDisabledBackground = Color.FromArgb("#919191");

        /// <summary>
        /// Gets the appropriate button foreground color based on the current theme.
        /// </summary>
        /// <param name="theme">The current application theme.</param>
        /// <returns>The color to use for button icons and text.</returns>
        public static Color GetButtonForegroundColor(AppTheme theme) =>
            theme == AppTheme.Dark ? DarkThemeButtonForeground : LightThemeButtonForeground;

        /// <summary>
        /// Gets the appropriate button background color based on enabled state.
        /// </summary>
        /// <param name="isEnabled">Whether the button is enabled.</param>
        /// <returns>The color to use for the button background.</returns>
        public static Color GetButtonBackgroundColor(bool isEnabled) =>
            isEnabled ? ButtonEnabledBackground : ButtonDisabledBackground;
    }
}
