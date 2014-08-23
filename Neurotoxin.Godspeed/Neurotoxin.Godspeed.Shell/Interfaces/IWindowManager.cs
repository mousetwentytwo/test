using System;
using System.Collections.Generic;
using System.Windows;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
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

        string ShowTextInputDialog(string title, string message, string defaultValue, IList<InputDialogOptionViewModel> options);
        object ShowListInputDialog(string title, string message, object defaultValue, IList<InputDialogOptionViewModel> options);
        bool ShowTreeSelectorDialog(ITreeSelectionViewModel viewModel);
        LoginDialogResult ShowLoginDialog(ILoginViewModel viewModel);
        bool Confirm(string title, string message);
        bool ActivateWindowOf<TViewModel>();
        bool CloseWindowOf<TViewModel>();
        bool CloseWindowOf(Type type);

        void ShowModelessWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : IView where TViewModel : class, IViewModel;

    }
}