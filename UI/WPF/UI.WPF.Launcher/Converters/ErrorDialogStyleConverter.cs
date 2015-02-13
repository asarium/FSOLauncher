using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FSOManagement.Annotations;
using ReactiveUI;

namespace UI.WPF.Launcher.Converters
{
    public class ErrorDialogStyleConverter : IValueConverter
    {
        [NotNull]
        public Style IsDefault { get; set; }

        [NotNull]
        public Style Default { get; set; }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Default;

            var command = value as RecoveryCommand;

            if (command == null)
                return Default;

            return command.IsDefault ? IsDefault : Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}