﻿using System;
using System.Windows;
using System.Windows.Controls;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Exceptions;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;

namespace Neurotoxin.Contour.Modules.FileManager.Views.Dialogs
{
    public partial class WriteErrorDialog : Window, ITransferErrorDialog
    {
        public TransferErrorDialogResult Result { get; private set; }

        public WriteErrorDialog(TransferException exception, FileSystemItemViewModel sourceFile, FileSystemItemViewModel targetFile)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            SourceFile.DataContext = sourceFile;
            TargetFile.DataContext = targetFile;
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