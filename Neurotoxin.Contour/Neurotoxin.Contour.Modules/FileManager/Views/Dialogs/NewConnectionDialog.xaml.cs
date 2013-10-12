using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Dialogs
{
    public partial class NewConnectionDialog : Window
    {
        public NewConnectionDialog(FtpConnectionItemViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (FtpConnectionItemViewModel) DataContext;
            viewModel.SetImageId(((ComboBoxItem)Version.SelectedItem).Tag.ToString());
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