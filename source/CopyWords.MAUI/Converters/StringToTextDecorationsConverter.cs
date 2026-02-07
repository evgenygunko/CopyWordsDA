using System.Globalization;

namespace CopyWords.MAUI.Converters
{
    public class StringToTextDecorationsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is string str && !string.IsNullOrWhiteSpace(str)
                ? TextDecorations.Underline
                : TextDecorations.None;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
