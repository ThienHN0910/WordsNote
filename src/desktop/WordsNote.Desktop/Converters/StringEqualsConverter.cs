using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WordsNote.Desktop.Converters;

public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var source = value?.ToString() ?? string.Empty;
        var expected = parameter?.ToString() ?? string.Empty;
        var isMatch = string.Equals(source, expected, StringComparison.OrdinalIgnoreCase);

        if (targetType == typeof(Visibility))
        {
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        return isMatch;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
