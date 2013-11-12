using System.Windows;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.ContentProviders;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    public abstract class DialogBase : BorderlessWindow
    {
        protected DialogBase()
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.Height;
            if (!UserSettings.DisableCustomChrome) Style = (Style)Application.Current.Resources["Dialog"];
            PreviewKeyDown += OnPreviewKeyDown;
        }

        protected virtual void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            e.Handled = true;
            DialogResult = false;
            Close();
        }

        protected virtual void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        protected virtual void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}