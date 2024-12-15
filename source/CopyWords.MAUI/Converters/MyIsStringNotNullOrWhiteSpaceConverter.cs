using System.Globalization;

namespace CopyWords.MAUI.Converters
{
    public class MyIsStringNotNullOrWhiteSpaceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value is string) && !string.IsNullOrEmpty((string)value);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
