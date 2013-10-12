using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Dialogs
{
    public partial class ReadErrorDialog : Window, ITransferErrorDialog
    {
        public TransferErrorDialogResult Result { get; private set; }

        public ReadErrorDialog(Exception exception)
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
                case "Retry":
                    Result = new TransferErrorDialogResult(CopyBehavior.Retry);
                    break;
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