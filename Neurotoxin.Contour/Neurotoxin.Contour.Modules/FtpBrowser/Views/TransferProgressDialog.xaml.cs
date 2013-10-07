using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class TransferProgressDialog : Window
    {
        public const string BytesFormat = "{0:#,0}";

        public TransferProgressDialog(FtpBrowserViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {

        }
    }
}