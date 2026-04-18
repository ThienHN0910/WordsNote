using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WordsNote.Desktop.Converters;

public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            var inverse = !flag;
            if (targetType == typeof(Visibility))
            {
                return inverse ? Visibility.Visible : Visibility.Collapsed;
            }

            return inverse;
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        if (value is bool flag)
        {
            return !flag;
        }

        return DependencyProperty.UnsetValue;
    }
}
