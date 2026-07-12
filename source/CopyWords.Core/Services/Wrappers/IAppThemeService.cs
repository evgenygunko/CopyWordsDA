namespace CopyWords.Core.Services.Wrappers
{
    public interface IAppThemeService
    {
        /// <summary>
        /// Gets the current application theme.
        /// </summary>
        global::CopyWords.Core.Models.AppColorTheme CurrentTheme { get; }

        /// <summary>
        /// Event raised when the theme changes.
        /// </summary>
        event EventHandler<global::CopyWords.Core.Models.AppColorTheme>? ThemeChanged;

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The application color theme to apply.</param>
        void ApplyTheme(global::CopyWords.Core.Models.AppColorTheme theme);
    }
}
