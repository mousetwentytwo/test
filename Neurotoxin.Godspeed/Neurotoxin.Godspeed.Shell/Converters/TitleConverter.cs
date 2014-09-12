using System;
using System.Globalization;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class TitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as FileSystemItemViewModel;
            if (item == null) return null;

            if (!string.IsNullOrEmpty(item.Title) && !item.IsUpDirectory) return item.Title;
            var name = item.Name;
            if (item.IsUpDirectory) name = Strings.UpDirectory;
            var format = item.Type == ItemType.Directory ? "[{0}]" : "{0}";
            return string.Format(format, name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}