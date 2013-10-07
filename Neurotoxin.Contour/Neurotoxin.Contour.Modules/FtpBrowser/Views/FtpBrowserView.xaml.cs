using System;
using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class FtpBrowserView : ModuleViewBase
    {
        private TransferProgressDialog _transferProgressDialog;

        public new FtpBrowserViewModel ViewModel
        {
            get { return (FtpBrowserViewModel)base.ViewModel; }
        }

        public FtpBrowserView(FtpBrowserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            eventAggregator.GetEvent<TransferStartedEvent>().Subscribe(ShowTransferProgressDialog);
            eventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(HideTransferProgressDialog);
        }

        private void ShowTransferProgressDialog(TransferStartedEventArgs transferStartedEventArgs)
        {
            if (_transferProgressDialog == null) _transferProgressDialog = new TransferProgressDialog(ViewModel);
            _transferProgressDialog.Show();
        }

        private void HideTransferProgressDialog(TransferFinishedEventArgs transferFinishedEventArgs)
        {
            _transferProgressDialog.Hide();
        }
    }
}