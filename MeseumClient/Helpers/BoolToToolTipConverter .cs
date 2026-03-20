using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MeseumClient.Helpers
{
    public class BoolToToolTipConverter : MarkupExtension, IValueConverter
    {
        private static BoolToToolTipConverter _instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string options)
            {
                var parts = options.Split('|');
                return (bool)value ? parts[0] : parts[1];
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new BoolToToolTipConverter());
        }
    }
}