namespace CopyWords.Core.Services.Wrappers
{
    public interface IAppThemeService
    {
        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme to apply (Light or Dark).</param>
        void ApplyTheme(AppTheme theme);
    }
}
