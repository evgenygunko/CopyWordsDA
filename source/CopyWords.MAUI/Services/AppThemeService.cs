using CopyWords.Core.Services.Wrappers;
using CopyWords.MAUI.Resources.Styles;

namespace CopyWords.MAUI.Services
{
    public class AppThemeService : IAppThemeService
    {
        public global::CopyWords.Core.Models.AppColorTheme CurrentTheme { get; private set; } = global::CopyWords.Core.Models.AppColorTheme.Blue;

        public event EventHandler<global::CopyWords.Core.Models.AppColorTheme>? ThemeChanged;

        public void ApplyTheme(global::CopyWords.Core.Models.AppColorTheme theme)
        {
            if (Application.Current?.Resources.MergedDictionaries == null)
            {
                return;
            }

            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            if (CurrentTheme == theme && ContainsTheme(mergedDictionaries, theme))
            {
                return;
            }

            // Find the current theme dictionary
            var existingTheme = mergedDictionaries.FirstOrDefault(d => d is BlueTheme or GraphiteTheme or DarkTheme);

            // Create the new theme dictionary
            ResourceDictionary newTheme = theme switch
            {
                global::CopyWords.Core.Models.AppColorTheme.Dark => new DarkTheme(),
                global::CopyWords.Core.Models.AppColorTheme.Graphite => new GraphiteTheme(),
                _ => new BlueTheme()
            };

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
            Application.Current.UserAppTheme = theme == global::CopyWords.Core.Models.AppColorTheme.Dark ? AppTheme.Dark : AppTheme.Light;

            // Update the current theme and raise the event
            CurrentTheme = theme;
            ThemeChanged?.Invoke(this, theme);
        }

        private static bool ContainsTheme(IEnumerable<ResourceDictionary> dictionaries, global::CopyWords.Core.Models.AppColorTheme theme) =>
            dictionaries.Any(dictionary => theme switch
            {
                global::CopyWords.Core.Models.AppColorTheme.Dark => dictionary is DarkTheme,
                global::CopyWords.Core.Models.AppColorTheme.Graphite => dictionary is GraphiteTheme,
                _ => dictionary is BlueTheme
            });
    }
}
