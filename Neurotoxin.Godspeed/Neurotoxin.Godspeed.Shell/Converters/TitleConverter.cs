using System;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class TitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) return null;
            var title = (string) values[0];
            var titleid = (string)values[1];

            if (!string.IsNullOrEmpty(title)) return title;
            var format = values[2] != DependencyProperty.UnsetValue && ((ItemType) values[2]) == ItemType.Directory
                             ? "[{0}]"
                             : "{0}";
            return string.Format(format, titleid);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}