using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace UI.WPF.Modules.Mods.Converters
{
    public class BitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                var bitmapImage = new BitmapImage(new Uri((string) value, UriKind.Absolute));

                return bitmapImage;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}