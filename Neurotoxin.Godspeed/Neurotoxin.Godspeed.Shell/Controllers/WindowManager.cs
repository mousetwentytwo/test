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
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Properties;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using System.Linq;

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
        private readonly Dictionary<Type, Window> _windows = new Dictionary<Type, Window>(); 
        private readonly Dictionary<FtpTraceListener, FtpTraceWindow> _traceWindows = new Dictionary<FtpTraceListener, FtpTraceWindow>();

        public WindowManager(IEventAggregator eventAggregator)
        {
            _container = UnityInstance.Container;
            _eventAggregator = eventAggregator;

            eventAggregator.GetEvent<TransferStartedEvent>().Subscribe(OnTransferStarted);
            eventAggregator.GetEvent<TransferFinishedEvent>().Subscribe(OnTransferFinished);
            eventAggregator.GetEvent<MigrationStartedEvent>().Subscribe(OnMigrationStarted);
            eventAggregator.GetEvent<MigrationFinishedEvent>().Subscribe(OnMigrationFinished);
            eventAggregator.GetEvent<FreestyleDatabaseCheckedEvent>().Subscribe(OnFreestyleDatabaseChecked);
            eventAggregator.GetEvent<ShowFtpTraceWindowEvent>().Subscribe(OnShowFtpTraceWindow);
            eventAggregator.GetEvent<CloseFtpTraceWindowEvent>().Subscribe(OnCloseFtpTraceWindow);
        }

        public void ShowErrorMessage(Exception exception)
        {
            UIThread.Run(() => ShowErrorMessageInner(exception));
        }

        private void ShowErrorMessageInner(Exception exception)
        {
            var errorDialog = new ErrorMessage(exception);
            errorDialog.ShowDialog();
        }

        public TransferErrorDialogResult ShowIoErrorDialog(Exception exception)
        {
            var dialog = new IoErrorDialog(exception);
            return dialog.ShowDialog() == true ? dialog.Result : null;
        }

        public TransferErrorDialogResult ShowWriteErrorDialog(string sourcePath, string targetPath, CopyAction disableFlags, Action preAction)
        {
            var dialog = new WriteErrorDialog(_eventAggregator, sourcePath, targetPath, disableFlags);
            if (preAction != null) preAction.Invoke();
            return dialog.ShowDialog() == true ? dialog.Result : null;
        }

        public bool? ShowReconnectionDialog(Exception exception)
        {
            var reconnectionDialog = new ReconnectionDialog(exception);
            return reconnectionDialog.ShowDialog();
        }

        public void ShowMessage(string title, string message, NotificationMessageFlags flags = NotificationMessageFlags.None)
        {
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

        public string ShowTextInputDialog(string title, string message, string defaultValue, IList<InputDialogOptionViewModel> options)
        {
            var viewModel = new InputDialogViewModel
            {
                Title = title,
                Message = message,
                DefaultValue = defaultValue,
                Options = options,
                Mode = InputDialogMode.ComboBox
            };
            var dialog = new InputDialog(viewModel);

            if (dialog.ShowDialog() == true)
            {
                var selectedItem = dialog.ComboBox.SelectedItem;
                if (selectedItem != null)
                {
                    var selectedValue = ((InputDialogOptionViewModel)dialog.ComboBox.SelectedItem);
                    if (selectedValue.DisplayName == dialog.ComboBox.Text) return (string)selectedValue.Value;
                }
                return dialog.ComboBox.Text;
            }
            return null;
        }

        public object ShowListInputDialog(string title, string message, object defaultValue, IList<InputDialogOptionViewModel> options)
        {
            if (options == null) throw new ArgumentException();

            var viewModel = new InputDialogViewModel
            {
                Title = title,
                Message = message,
                DefaultValue = defaultValue,
                Options = options,
                Mode = InputDialogMode.RadioGroup
            };

            var dialog = new InputDialog(viewModel);

            dialog.ShowDialog();
            return options.First(o => o.IsSelected).Value;
        }


        public bool ShowTreeSelectorDialog(ITreeSelectionViewModel viewModel)
        {
            Window owner = null;
            if (viewModel.Parent != null)
            {
                var parentType = viewModel.Parent.GetType();
                if (_windows.ContainsKey(parentType)) owner = _windows[parentType];
            }
            var dialog = new TreeSelectionDialog(viewModel) { Owner = owner };
            return dialog.ShowDialog() == true;
        }

        public LoginDialogResult ShowLoginDialog(ILoginViewModel viewModel)
        {
            var dialog = new LoginDialog(viewModel);
            if (dialog.ShowDialog() != true) return null;

            return new LoginDialogResult
            {
                Username = viewModel.Username,
                Password = viewModel.Password,
                RememberPassword = viewModel.IsRememberPasswordEnabled ? (bool?)viewModel.RememberPassword : null
            };
        }

        public bool Confirm(string title, string message)
        {
            return new ConfirmationDialog(title, message).ShowDialog() == true;
        }

        public bool ActivateWindowOf<TViewModel>()
        {
            var type = typeof (TViewModel);
            if (!_windows.ContainsKey(type)) return false;
            _windows[type].Activate();
            return true;
        }

        public bool CloseWindowOf<TViewModel>()
        {
            var type = typeof(TViewModel);
            if (!_windows.ContainsKey(type)) return false;
            _windows[type].Close();
            return true;
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
            if (!vm.IsResumeSupported)
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

        private void OnMigrationStarted(MigrationStartedEventArgs e)
        {
            MainWindowHitTestVisible(false);
            _progressDialog = _container.Resolve<ProgressDialog>(new ParameterOverride("viewModel", e.ViewModel));
            _progressDialog.Show();
        }

        private void OnMigrationFinished(MigrationFinishedEventArgs e)
        {
            _progressDialog.Close();
            _progressDialog = null;
            MainWindowHitTestVisible(true);
        }

        private void OnFreestyleDatabaseChecked(FreestyleDatabaseCheckedEventArgs e)
        {
            var vm = e.ViewModel;
            if (vm.HasMissingEntries || vm.HasMissingFolders)
            {
                ShowModelessWindow<FreestyleDatabaseCheckerWindow, FreestyleDatabaseCheckerViewModel>(vm);
            }
            else
            {
                ShowMessage(Resx.ErrorsInFreestyleDatabase, Resx.NoErrorsInFreestyleDatabase);
            }
        }

        private void ShowModelessWindow<TWindow, TViewModel>(TViewModel vm) where TWindow : Window, IView where TViewModel : class, IViewModel
        {
            var w = _container.Resolve<TWindow, TViewModel>(vm);
            _windows.Add(typeof(FreestyleDatabaseCheckerViewModel), w);
            w.Closed += OnModelessWindowClosed;
            w.Show();
            
        }

        private void OnModelessWindowClosed(object sender, EventArgs e)
        {
            var w = _windows.FirstOrDefault(kvp => kvp.Value.Equals(sender));
            w.Value.Closed -= OnModelessWindowClosed;
            _windows.Remove(w.Key);
        }

        private void OnShowFtpTraceWindow(ShowFtpTraceWindowEventArgs e)
        {
            FtpTraceWindow window;
            if (_traceWindows.ContainsKey(e.TraceListener))
            {
                window = _traceWindows[e.TraceListener];
                window.Activate();
                return;
            }

            var viewModel = new FtpTraceViewModel(e.TraceListener, e.ConnectionName);
            window = new FtpTraceWindow(viewModel);
            _traceWindows.Add(e.TraceListener, window);
            window.Show();
        }

        private void OnCloseFtpTraceWindow(CloseFtpTraceWindowEventArgs e)
        {
            if (!_traceWindows.ContainsKey(e.TraceListener)) return;
            var window = _traceWindows[e.TraceListener];
            window.Close();
            _traceWindows.Remove(e.TraceListener);
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