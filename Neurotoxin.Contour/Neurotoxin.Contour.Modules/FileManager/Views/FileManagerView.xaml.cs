using System;
using System.Windows;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;
using Neurotoxin.Contour.Modules.FileManager.Views.Dialogs;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.Views
{
    public partial class FileManagerView : ModuleViewBase
    {
        private TransferProgressDialog _transferProgressDialog;

        public new FileManagerViewModel ViewModel
        {
            get { return (FileManagerViewModel)base.ViewModel; }
        }

        public FileManagerView(FileManagerViewModel viewModel)
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