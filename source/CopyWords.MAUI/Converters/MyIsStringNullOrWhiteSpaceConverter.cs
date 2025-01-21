using System.Globalization;

namespace CopyWords.MAUI.Converters
{
    public class MyIsStringNullOrWhiteSpaceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value is not string) || (value is null) || string.IsNullOrEmpty((string)value);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
