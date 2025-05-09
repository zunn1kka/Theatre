using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Theatre.Pages.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool paramValue))
            {
                invert = paramValue;
            }

            bool isNull = value == null || (value is DateTime date && date == default);

            return (isNull ^ invert) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}