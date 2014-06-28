using System;
using System.Collections.Generic;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IWindowManager
    {
        void ShowErrorMessage(Exception exception);
        TransferErrorDialogResult ShowIoErrorDialog(Exception exception);
        TransferErrorDialogResult ShowWriteErrorDialog(string sourcePath, string targetPath, CopyAction disableFlags, Action preAction);
        bool? ShowReconnectionDialog(Exception exception);
        void ShowMessage(string title, string message, NotificationMessageFlags flags = NotificationMessageFlags.None);
        void CloseMessage();

        string ShowTextInputDialog(string title, string message, string defaultValue, IList<InputDialogOptionViewModel> options = null);
        bool ShowTreeSelectorDialog(ITreeSelectionViewModel viewModel);
    }
}