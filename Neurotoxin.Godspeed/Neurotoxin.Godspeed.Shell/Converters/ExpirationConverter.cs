using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class ExpirationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;
            return intValue == 0 ? "Never" : string.Format("{0} day{1}", intValue, intValue > 1 ? "s" : string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
