using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    public class BorderlessWindow : Window
    {
        protected IUserSettings UserSettings { get; private set; }

        protected BorderlessWindow()
        {
            UserSettings = UnityInstance.Container.Resolve<IUserSettings>();
            Icon = new BitmapImage(new Uri("pack://application:,,,/Neurotoxin.Godspeed.Shell;component/Resources/window.ico"));
            SnapsToDevicePixels = true;
            Background = (SolidColorBrush) Application.Current.Resources["ControlBackgroundBrush"];
            if (!UserSettings.DisableCustomChrome) Style = (Style)Application.Current.Resources["Window"];
        }
    }
}