using System.Windows;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Dialogs
{
    public partial class TransferProgressDialog
    {
        public const string BytesFormat = "{0:#,0} / {1:#,0} Bytes";
        public const string TitleFormat = "{0} ({1}%)";
        public const string FileCountFormat = "{0} / {1}";
        public const string SpeedFormat = "{0} KBps";
        public const string TimeFormat = "{0:hh\\:mm\\:ss} / {1:hh\\:mm\\:ss}";

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
            Hide();
        }
    }
}