using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class WriteErrorDialog : Window, ITransferErrorDialog
    {
        public TransferErrorDialogResult Result { get; private set; }

        public WriteErrorDialog(Exception exception)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ErrorMessage.Text = exception.Message;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Overwrite":
                    Result = new TransferErrorDialogResult(CopyBehavior.Retry, CopyActionScope.Current, CopyAction.Overwrite);
                    break;
                case "OverwriteAll":
                    Result = new TransferErrorDialogResult(CopyBehavior.Retry, CopyActionScope.All, CopyAction.Overwrite);
                    break;
                case "OverwriteAllOlder":
                    Result = new TransferErrorDialogResult(CopyBehavior.Retry, CopyActionScope.All, CopyAction.OverwriteOlder);
                    break;
                case "Resume":
                    Result = new TransferErrorDialogResult(CopyBehavior.Retry, CopyActionScope.Current, CopyAction.Resume);
                    break;
                case "ResumeAll":
                    Result = new TransferErrorDialogResult(CopyBehavior.Retry, CopyActionScope.All, CopyAction.Resume);
                    break;
                case "Rename":
                    throw new NotSupportedException();
                case "Skip":
                    Result = new TransferErrorDialogResult(CopyBehavior.Skip);
                    break;
                case "SkipAll":
                    Result = new TransferErrorDialogResult(CopyBehavior.Skip, CopyActionScope.All);
                    break;
                case "Cancel":
                    Result = new TransferErrorDialogResult(CopyBehavior.Cancel);
                    break;
            }
            DialogResult = true;
            Close();
        }
    }
}