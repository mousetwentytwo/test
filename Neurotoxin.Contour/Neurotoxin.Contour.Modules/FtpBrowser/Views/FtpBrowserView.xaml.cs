using System;
using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;
using Neurotoxin.Contour.Modules.FtpBrowser.Views.Dialogs;
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
            viewModel.TransferStarted += ViewModelOnTransferStarted;
            viewModel.TransferFinished += ViewModelOnTransferFinished;
        }

        private void ViewModelOnTransferStarted()
        {
            if (_transferProgressDialog == null) _transferProgressDialog = new TransferProgressDialog(ViewModel);
            _transferProgressDialog.Show();
        }

        private void ViewModelOnTransferFinished()
        {
            _transferProgressDialog.Hide();
        }
    }
}