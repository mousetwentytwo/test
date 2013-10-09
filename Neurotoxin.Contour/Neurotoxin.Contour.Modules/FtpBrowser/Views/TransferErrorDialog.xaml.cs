using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Views
{
    public partial class TransferErrorDialog : Window
    {
        public TransferErrorDialogResult Result { get; private set; }

        public TransferErrorDialog(Exception exception)
        {
            InitializeComponent();
            ErrorMessage.Text = exception.Message;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.Name)
            {
                case "Overwrite":
                    Result = new TransferErrorDialogResult(CopyBehavior.Overwrite, CopyBehaviorScope.Current);
                    break;
                case "OverwriteAll":
                    Result = new TransferErrorDialogResult(CopyBehavior.Overwrite, CopyBehaviorScope.All);
                    break;
                case "OverwriteAllOlder":
                    Result = new TransferErrorDialogResult(CopyBehavior.Overwrite, CopyBehaviorScope.AllOlder);
                    break;
                case "Resume":
                    Result = new TransferErrorDialogResult(CopyBehavior.Resume, CopyBehaviorScope.Current);
                    break;
                case "ResumeAll":
                    Result = new TransferErrorDialogResult(CopyBehavior.Resume, CopyBehaviorScope.All);
                    break;
                case "Rename":
                    throw new NotSupportedException();
                case "Skip":
                    Result = new TransferErrorDialogResult(CopyBehavior.Skip, CopyBehaviorScope.Current);
                    break;
                case "SkipAll":
                    Result = new TransferErrorDialogResult(CopyBehavior.Skip, CopyBehaviorScope.All);
                    break;
                case "Cancel":
                    Result = new TransferErrorDialogResult(CopyBehavior.Cancel, CopyBehaviorScope.All);
                    break;
            }
            DialogResult = true;
            Close();
        }
    }
}