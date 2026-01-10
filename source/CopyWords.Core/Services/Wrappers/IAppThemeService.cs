namespace CopyWords.Core.Services.Wrappers
{
    public interface IAppThemeService
    {
        /// <summary>
        /// Gets the current application theme.
        /// </summary>
        AppTheme CurrentTheme { get; }

        /// <summary>
        /// Event raised when the theme changes.
        /// </summary>
        event EventHandler<AppTheme>? ThemeChanged;

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme to apply (Light or Dark).</param>
        void ApplyTheme(AppTheme theme);
    }
}
