using System.Windows;
using System.Windows.Input;

namespace Neurotoxin.Godspeed.Presentation.Controls
{
    public abstract class DialogBase : BorderlessWindow
    {
        protected DialogBase()
        {
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