using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Converters
{
    public class SizeConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue) return null;
            var type = (ItemType) values[0];
            var subtype = (ItemSubtype)values[1];
            var size = (long?) values[2];

            return size == null
                       ? string.Format("<{0}>",
                                       subtype != ItemSubtype.Undefined
                                           ? subtype.ToString().ToUpper()
                                           : type.ToString().ToUpper())
                       : size.Value.ToString("0,0", CultureInfo.InvariantCulture);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}