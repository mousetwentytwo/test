using System.Windows;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class InputDialog : Window
    {
        public InputDialog(string title, string message, string defaultValue)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Title = title;
            Message.Text = message;
            Input.Text = defaultValue;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}