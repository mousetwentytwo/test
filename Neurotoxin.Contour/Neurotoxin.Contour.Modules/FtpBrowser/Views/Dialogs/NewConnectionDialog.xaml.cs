using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views.Dialogs
{
    public partial class NewConnectionDialog : Window
    {
        public NewConnectionDialog(FtpConnectionItemViewModel viewModel)
        {
            DataContext = viewModel;
            Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (FtpConnectionItemViewModel) DataContext;
            viewModel.SetImageId(((ComboBoxItem)Version.SelectedItem).Tag.ToString());
            viewModel.Password = Password.Password;
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