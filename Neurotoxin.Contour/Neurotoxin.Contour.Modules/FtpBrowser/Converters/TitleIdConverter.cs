using System;
using System.Windows;
using System.Windows.Data;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Converters
{
    public class TitleIdConverter : IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) return null;
            var title = (string)values[0];
            var titleid = (string)values[1];

            return titleid != null && title != null ? string.Format(" [{0}]", titleid) : null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}