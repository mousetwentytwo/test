using System;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class TransferTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var e = (TransferType)value;
            return e == TransferType.Copy || e == TransferType.Move
                       ? Visibility.Visible
                       : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}