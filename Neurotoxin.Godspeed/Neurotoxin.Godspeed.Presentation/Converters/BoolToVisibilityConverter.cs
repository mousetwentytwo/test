using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    /// <summary>
    /// Bool to Visibility Converter. If converter parameter is set to true the result will be inversed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        private bool ParseParameter(object parameter)
        {
            bool p = false;
            if (parameter != null)
            {
                if (parameter is string) Boolean.TryParse((string)parameter, out p);
                else if (parameter is bool) p = (bool)parameter;
            }
            return p;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool v = false;
            if (value != null) v = !(value is bool) || (bool)value;
            if (ParseParameter(parameter)) v = !v;
            return (v ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility v = (Visibility)value;
            return (v == Visibility.Visible);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //TODO: or

            var v = true;
            foreach (var value in values)
            {
                if (value == DependencyProperty.UnsetValue || value == null || (value is bool && !(bool)value))
                {
                    v = false;
                    break;
                }
                
            }
            if (ParseParameter(parameter)) v = !v;
            return (v ? Visibility.Visible : Visibility.Collapsed);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}