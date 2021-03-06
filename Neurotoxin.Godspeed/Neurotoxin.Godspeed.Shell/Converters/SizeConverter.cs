﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Neurotoxin.Godspeed.Shell.Constants;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

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

            var type = (ItemType)values[1];
            var titleType = (TitleType)values[2];
            var isRefreshing = (bool)values[3];

            if (isRefreshing) return "?";

            //size is set
            if (values[0] != null) return Convert(values[0], targetType, parameter, culture);

            string resxName;
            if (titleType != TitleType.Unknown)
                resxName = titleType.GetType().Name + titleType;
            else
                resxName = type.GetType().Name + type;

            return string.Format("<{0}>", Resx.ResourceManager.GetString(resxName).ToUpper());
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}