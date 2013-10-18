using System;
using System.Windows;
using System.Windows.Data;

namespace Neurotoxin.Godspeed.Presentation.Converters
{
    /// <summary>
    /// Bool to Visibility Converter. If converter parameter is set to true the result will be inversed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool v = false;
            if (value != null) v = !(value is bool) || (bool)value;
            bool p = false;
            if (parameter != null)
            {
                if (parameter is string) Boolean.TryParse((string)parameter, out p);
                else if (parameter is bool) p = (bool)parameter;
                else p = value != null;
            }
            if (p) v = !v;
            return (v ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility v = (Visibility)value;
            return (v == Visibility.Visible);
        }

        #endregion
    }
}