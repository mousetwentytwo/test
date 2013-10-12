using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views.Dialogs
{
    public partial class TransferProgressDialog : Window
    {
        public const string BytesFormat = "{0:#,0}";

        private FtpBrowserViewModel ViewModel
        {
            get { return (FtpBrowserViewModel) DataContext; }
        }

        public TransferProgressDialog(FtpBrowserViewModel viewModel)
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