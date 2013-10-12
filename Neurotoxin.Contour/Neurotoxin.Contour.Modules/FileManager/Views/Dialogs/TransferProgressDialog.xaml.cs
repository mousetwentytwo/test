using System.Windows;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Dialogs
{
    public partial class TransferProgressDialog : Window
    {
        public const string BytesFormat = "{0:#,0}";

        private FileManagerViewModel ViewModel
        {
            get { return (FileManagerViewModel) DataContext; }
        }

        public TransferProgressDialog(FileManagerViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AbortTransfer();
        }
    }
}