using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MeseumClient.Helpers
{
    public class WindowRatioToColumnWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth)
            {
                // Если окно узкое (<600px) — левая колонка исчезает
                if (totalWidth < 600)
                    return new GridLength(0);
                else
                    return new GridLength(2, GridUnitType.Star);
            }

            // По умолчанию 2*
            return new GridLength(2, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}