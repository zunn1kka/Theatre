using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Theatre.Pages.Converters
{
    public class TypeIdToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            int userTypeId;
            int requiredTypeId;

            if (int.TryParse(value.ToString(), out userTypeId) &&
                int.TryParse(parameter.ToString(), out requiredTypeId))
            {
                return userTypeId == requiredTypeId ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}