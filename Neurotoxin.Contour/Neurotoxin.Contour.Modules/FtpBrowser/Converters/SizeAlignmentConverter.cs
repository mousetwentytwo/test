using System;
using System.Windows;
using System.Windows.Data;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Converters
{
    [ValueConversion(typeof(long?), typeof(HorizontalAlignment))]
    public class SizeAlignmentConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var size = (long?) value;
            return size == null ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}