using CopyWords.Core.Services.Wrappers;
using CopyWords.MAUI.Resources.Styles;

namespace CopyWords.MAUI.Services
{
    public class AppThemeService : IAppThemeService
    {
        public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

        public event EventHandler<AppTheme>? ThemeChanged;

        public void ApplyTheme(AppTheme theme)
        {
            if (Application.Current?.Resources.MergedDictionaries == null)
            {
                return;
            }

            // Exit early if the theme is already applied
            if (Application.Current.UserAppTheme == theme)
            {
                return;
            }

            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // Find the current theme dictionary
            var existingTheme = mergedDictionaries.FirstOrDefault(d => d is LightTheme or DarkTheme);

            // Create the new theme dictionary
            ResourceDictionary newTheme = theme == AppTheme.Dark ? new DarkTheme() : new LightTheme();

            if (existingTheme != null && mergedDictionaries is IList<ResourceDictionary> list)
            {
                // Replace at the same index to maintain proper ordering
                int existingIndex = list.IndexOf(existingTheme);
                list[existingIndex] = newTheme;
            }
            else if (existingTheme != null)
            {
                // Fallback: remove old and add new
                mergedDictionaries.Remove(existingTheme);
                mergedDictionaries.Add(newTheme);
            }
            else
            {
                // No existing theme, just add
                mergedDictionaries.Add(newTheme);
            }

            // Update the UserAppTheme to match
            Application.Current.UserAppTheme = theme;

            // Update the current theme and raise the event
            CurrentTheme = theme;
            ThemeChanged?.Invoke(this, theme);
        }
    }
}
