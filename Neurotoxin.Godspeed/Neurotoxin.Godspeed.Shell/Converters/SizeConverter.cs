using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class SizeConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (long?)value;
            return string.Format("{0:#,0} {1}", size.Value, parameter).Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue) return null;
            
            var type = (ItemType) values[1];
            var contentType = (TitleType)values[2];

            return values[0] == null
                       ? string.Format("<{0}>", contentType != TitleType.Unknown ? contentType.ToString().ToUpper() : type.ToString().ToUpper())
                       : Convert(values[0], targetType, parameter, culture);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}