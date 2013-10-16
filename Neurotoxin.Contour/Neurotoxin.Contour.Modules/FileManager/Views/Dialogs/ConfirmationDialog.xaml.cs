using System.Windows;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Dialogs
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string message)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Message.Text = message;
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