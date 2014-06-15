using System.Windows;
using System.Windows.Input;

namespace Neurotoxin.Godspeed.Shell.Primitives
{
    public abstract class DialogBase : BorderlessWindow
    {
        public static readonly DependencyProperty CloseButtonVisibilityProperty = DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(DialogBase), new PropertyMetadata(Visibility.Visible));

        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        protected DialogBase()
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.Height;
            if (!UserSettings.DisableCustomChrome) Style = (Style)Application.Current.Resources["Dialog"];
            PreviewKeyDown += OnPreviewKeyDown;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        protected virtual void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsVisible || !IsActive || e.Key != Key.Escape) return;
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