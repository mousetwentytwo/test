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
            if (HasError()) return;
            DialogResult = true;
            Close();
        }

        private bool HasError()
        {
            var result = false;
            var controls = new[] {Name, Address, Port, Username, Password};
            foreach (var control in controls)
            {
                control.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                if (Validation.GetHasError(control)) result = true;
            }
            return result;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}