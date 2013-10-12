using System;
using System.Windows;
using System.Windows.Data;

namespace Neurotoxin.Contour.Modules.FileManager.Converters
{
    public class NameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) return null;
            var title = (string)values[0];
            var name = (string)values[1];

            return name == title ? null : (name != null && title != null ? string.Format(" [{0}]", name) : null);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}