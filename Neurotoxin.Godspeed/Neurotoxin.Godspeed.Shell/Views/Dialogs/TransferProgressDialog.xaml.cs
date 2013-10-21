using System.Windows;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class TransferProgressDialog
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

        protected override void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AbortTransfer();
            base.CancelButtonClick(sender, e);
        }
    }
}