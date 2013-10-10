using System;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Converters
{
    public class TransferProgressDialogModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var e = (TransferProgressDialogMode)value;
            return e == TransferProgressDialogMode.Copy || e == TransferProgressDialogMode.Move
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}