using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UI.WPF.Modules.Update.Converters
{
    public class BorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
                return Brushes.Transparent;
            else
                return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}