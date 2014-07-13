﻿using System;
using System.Collections.Generic;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Extensions;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class ConsoleWriter : IWindowManager
    {
        public Func<TransferErrorDialogResult> WriteErrorDialogResult { get; set; }

        public void ShowErrorMessage(Exception exception)
        {
            Console.WriteLine("[Error] {0}{1}{2}", exception.Message, Environment.NewLine, exception.StackTrace);
        }

        public TransferErrorDialogResult ShowIoErrorDialog(Exception exception)
        {
            Console.WriteLine("[Error] {0}{1}{2}", exception.Message, Environment.NewLine, exception.StackTrace);
            return new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
        }

        public TransferErrorDialogResult ShowWriteErrorDialog(string sourcePath, string targetPath, CopyAction disableflags, Action preAction)
        {
            Console.WriteLine("[Error] File already exists. (S: {0}, T: {1})", sourcePath, targetPath);
            return WriteErrorDialogResult != null ? WriteErrorDialogResult.Invoke() : new TransferErrorDialogResult(ErrorResolutionBehavior.Cancel);
        }

        public bool? ShowReconnectionDialog(Exception exception)
        {
            Console.WriteLine("[Error] Connection lost.");
            return false;
        }

        public void ShowMessage(string title, string message, NotificationMessageFlags flags = NotificationMessageFlags.None)
        {
            Console.WriteLine("[{0}Message] Title: {1}, Text: {2}", flags == NotificationMessageFlags.NonClosable ? "Open " : string.Empty, title, message);
        }

        public void CloseMessage()
        {
            Console.WriteLine("[Close Message]");
        }

        public string ShowTextInputDialog(string title, string message, string defaultValue, IList<InputDialogOptionViewModel> options)
        {
            Console.WriteLine("[Input] Title: {0} Message: {1}, Value: {2}", title, message, defaultValue);
            if (options != null) return options.Random().Value.ToString();
            return C.Random<string>();
        }

        public object ShowListInputDialog(string title, string message, object defaultValue, IList<InputDialogOptionViewModel> options)
        {
            throw new NotImplementedException();
        }

        public bool ShowTreeSelectorDialog(ITreeSelectionViewModel viewModel)
        {
            return true;
        }

        public LoginDialogResult ShowLoginDialog(ILoginViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public bool IsMessageIgnored(string message)
        {
            return false;
        }

        public void IgnoreMessage(string message)
        {
        }

        public bool Confirm(string title, string message)
        {
            return true;
        }

        public bool ActivateWindowOf<TViewModel>()
        {
            return false;
        }

        public bool CloseWindowOf<TViewModel>()
        {
            return true;
        }
    }
}