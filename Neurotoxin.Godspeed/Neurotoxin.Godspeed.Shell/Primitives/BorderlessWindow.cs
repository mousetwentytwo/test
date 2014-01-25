using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Neurotoxin.Godspeed.Shell.ContentProviders;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    public class BorderlessWindow : Window
    {
        protected BorderlessWindow()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/Neurotoxin.Godspeed.Shell;component/Resources/window.ico"));
            SnapsToDevicePixels = true;
            Background = (SolidColorBrush) Application.Current.Resources["ControlBackgroundBrush"];
            if (!UserSettings.DisableCustomChrome) Style = (Style)Application.Current.Resources["Window"];
        }
    }
}