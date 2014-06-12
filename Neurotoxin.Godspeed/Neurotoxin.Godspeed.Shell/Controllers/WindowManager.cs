using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Properties;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.Controllers
{
    public class WindowManager : IWindowManager
    {
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private NotificationMessage _notificationMessage;
        private bool _isAbortionInProgress;
        private TransferProgressDialog _transferProgressDialog;
        private ProgressDialog _progressDialog;

        public WindowManager(IEventAggregator eventAggregator)
        {
            _container = UnityInstance.Container;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<TransferStartedEvent>().Subscribe(OnTransferStarted);
            eventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(OnTransferFinished);
            eventAggregator.GetEvent<CacheMigrationEvent>().Subscribe(OnCacheMigration);
            eventAggregator.GetEvent<FreestyleDatabaseCheckEvent>().Subscribe(OnFreestyleDatabaseCheck);
        }

        public void ShowErrorMessage(Exception exception)
        {
            ErrorMessage.Show(exception);
        }

        public TransferErrorDialogResult ShowIoErrorDialog(Exception exception)
        {
            var dialog = new IoErrorDialog(exception);
            return dialog.ShowDialog() == true ? dialog.Result : null;
        }

        public TransferErrorDialogResult ShowWriteErrorDialog(string sourcePath, string targetPath, bool isResumeSupported, IFileListPaneViewModel sourcePane, IFileListPaneViewModel targetPane)
        {
            var dialog = new WriteErrorDialog(_eventAggregator, sourcePath, targetPath, isResumeSupported);
            sourcePane.GetItemViewModel(sourcePath);
            targetPane.GetItemViewModel(targetPath);
            return dialog.ShowDialog() == true ? dialog.Result : null;
        }

        public bool? ShowReconnectionDialog(Exception exception)
        {
            var reconnectionDialog = new ReconnectionDialog(exception);
            return reconnectionDialog.ShowDialog();
        }

        public void ShowMessage(string title, string message, NotificationMessageFlags flags = NotificationMessageFlags.None)
        {
            var userSettings = _container.Resolve<IUserSettings>();
            if (userSettings.IsMessageIgnored(message)) return;
            UIThread.Run(() => ShowMessageInner(title, message, flags));
        }

        private void ShowMessageInner(string title, string message, NotificationMessageFlags flags)
        {
            if (_notificationMessage != null) _notificationMessage.Close();

            _notificationMessage = new NotificationMessage(title, message, flags);
            _notificationMessage.ShowDialog();
        }

        public void CloseMessage()
        {
            UIThread.Run(CloseMessageInner);
        }

        private void CloseMessageInner()
        {
            if (_notificationMessage != null) _notificationMessage.Close();
            _notificationMessage = null;
        }

        public string ShowTextInputDialog(string title, string message, string defaultValue, IList<InputDialogOptionViewModel> options = null)
        {
            return InputDialog.ShowText(title, message, defaultValue, options);
        }

        private void OnTransferStarted(TransferStartedEventArgs e)
        {
            if (_transferProgressDialog == null)
            {
                _transferProgressDialog = new TransferProgressDialog(e.Sender as TransferManagerViewModel);
                _transferProgressDialog.Closing += TransferProgressDialogOnClosing;
                _transferProgressDialog.Closed += TransferProgressDialogOnClosed;
            }

            MainWindowHitTestVisible(false);
            _isAbortionInProgress = false;
            _transferProgressDialog.Show();
        }

        private void TransferProgressDialogOnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (_isAbortionInProgress) return;
            var vm = ((TransferProgressDialog) sender).ViewModel;
            if (!vm.TargetPane.IsResumeSupported)
            {
                var d = new ConfirmationDialog(Resources.Warning, Resources.ResumeIsNotAvailableConfirmation);
                if (d.ShowDialog() != true) return;
            }
            _transferProgressDialog.Abort.IsEnabled = false;
            _isAbortionInProgress = true;
            _transferProgressDialog.ViewModel.AbortTransfer();
        }

        private void TransferProgressDialogOnClosed(object sender, EventArgs e)
        {
            MainWindowHitTestVisible(true);
            _transferProgressDialog.Closed -= TransferProgressDialogOnClosed;
            _transferProgressDialog = null;
        }

        private void OnTransferFinished(TransferFinishedEventArgs e)
        {
            if (_transferProgressDialog != null)
            {
                _transferProgressDialog.Closing -= TransferProgressDialogOnClosing;
                _transferProgressDialog.Close();
            }
            if (e.Shutdown) Shutdown();
        }

        private void OnCacheMigration(CacheMigrationEventArgs e)
        {
            MainWindowHitTestVisible(false);
            _progressDialog = _container.Resolve<ProgressDialog, CacheMigrationViewModel>();
            var vm = (CacheMigrationViewModel)_progressDialog.ViewModel;
            vm.Finished += OnMigrationFinished;
            vm.Initialize(e);
            _progressDialog.Show();
        }

        private void OnMigrationFinished(IProgressViewModel sender)
        {
            sender.Finished -= OnMigrationFinished;
            MainWindowHitTestVisible(true);
            _progressDialog.Close();
            _progressDialog = null;
        }

        private void OnFreestyleDatabaseCheck(FreestyleDatabaseCheckEventArgs e)
        {
            MainWindowHitTestVisible(false);
            var vm = _container.Resolve<FreestyleDatabaseCheckerViewModel>(new ParameterOverride("parent", e.FtpContentViewModel));
            _progressDialog = _container.Resolve<ProgressDialog, FreestyleDatabaseCheckerViewModel>(vm);
            vm.Finished += OnFreestyleDatabaseCheckFinished;
            vm.Check();
            _progressDialog.Show();
        }

        private void OnFreestyleDatabaseCheckFinished(IProgressViewModel sender)
        {
            sender.Finished -= OnFreestyleDatabaseCheckFinished;
            MainWindowHitTestVisible(true);
            _progressDialog.Close();
            var vm = (FreestyleDatabaseCheckerViewModel)_progressDialog.ViewModel;
            var window = _container.Resolve<FreestyleDatabaseCheckerWindow, FreestyleDatabaseCheckerViewModel>(vm);
            window.ShowDialog();
            _progressDialog = null;
        }

        private void MainWindowHitTestVisible(bool value)
        {
            var w = Application.Current.MainWindow;
            w.IsHitTestVisible = value;
            if (value)
                w.PreviewKeyDown -= PreventAllKeyboardActions;
            else
                w.PreviewKeyDown += PreventAllKeyboardActions;
        }

        private static void PreventAllKeyboardActions(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private static void Shutdown()
        {
            var mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            var mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = "1";
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            }
        }
    }
}