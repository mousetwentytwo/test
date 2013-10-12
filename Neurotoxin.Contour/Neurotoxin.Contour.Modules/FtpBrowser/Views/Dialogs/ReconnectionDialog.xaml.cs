using System.Windows;
using System.Windows.Controls;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views.Dialogs
{
    public partial class ReconnectionDialog : Window
    {
        public const string WarningMessageFormat = "The connection with {0} has been lost.";

        public ReconnectionDialog()
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Reconnect":
                    DialogResult = true;
                    break;
                case "Cancel":
                    DialogResult = false;
                    break;
            }
            Close();
        }
    }
}