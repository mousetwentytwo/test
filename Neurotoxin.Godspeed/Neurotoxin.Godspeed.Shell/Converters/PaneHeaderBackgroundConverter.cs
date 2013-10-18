using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class PaneHeaderBackgroundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isActive = (bool) value;
            return new SolidColorBrush(isActive ? Colors.Blue : Colors.SkyBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}