using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MeseumClient.Helpers
{
    public class PasswordVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPasswordVisible = values.Length > 0 && values[0] is bool visible && visible;
            bool isPasswordHidden = values.Length > 1 && values[1] is bool hidden && hidden;

            bool inverse = parameter?.ToString() == "Inverse";

            if (!isPasswordVisible) return Visibility.Collapsed;

            return inverse ? (isPasswordHidden ? Visibility.Collapsed : Visibility.Visible)
                           : (isPasswordHidden ? Visibility.Visible : Visibility.Collapsed);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}