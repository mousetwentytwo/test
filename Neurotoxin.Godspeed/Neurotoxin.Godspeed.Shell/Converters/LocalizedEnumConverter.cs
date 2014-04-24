using System;
using System.Globalization;
using System.Windows.Data;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class LocalizedEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value.GetType();
            if (!type.IsEnum) return value;
            var name = Enum.GetName(type, value);
            var key = type.Name + name;
            return Resx.ResourceManager.GetString(key) ?? "Key: " + key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}