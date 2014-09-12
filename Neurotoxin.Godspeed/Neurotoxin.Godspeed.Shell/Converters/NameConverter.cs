using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as FileSystemItemViewModel;
            if (item == null) return null;
            var title = item.Title;
            var name = item.Name;

            return name == title || item.IsUpDirectory
                ? null
                : (name != null && title != null ? string.Format(" [{0}]", name) : null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}