using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Core.Net;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Commands;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using System.Linq;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Views
{
    public partial class FileManagerWindow
    {
        private readonly IEventAggregator _eventAggregator;
        private bool _isAbortionInProgress;
        private TransferProgressDialog _transferProgressDialog;
        private MigrationProgressDialog _migrationProgressDialog;
        private Queue<Timer> _userMessageReadTimers;

        public FileManagerViewModel ViewModel
        {
            get { return (FileManagerViewModel) DataContext; }
        }

        public FileManagerWindow(FileManagerViewModel viewModel, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            var assembly = Assembly.GetAssembly(typeof(FileManagerWindow));
            var assemblyName = assembly.GetName();
            var version = assemblyName.Version;
            Title = String.Format("GODspeed v{0}", version);

            InitializeComponent();
            DataContext = viewModel;
            viewModel.TransferStarted += ViewModelOnTransferStarted;
            viewModel.TransferFinished += ViewModelOnTransferFinished;
            CommandBindings.Add(new CommandBinding(FileManagerCommands.OpenDriveDropdownCommand, ExecuteOpenDriveDropdownCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.SettingsCommand, ExecuteSettingsCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.StatisticsCommand, ExecuteStatisticsCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.AboutCommand, ExecuteAboutCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.VisitWebsiteCommand, ExecuteVisitWebsiteCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.UserStatisticsParticipationCommand, ExecuteUserStatisticsParticipationCommand));
            CommandBindings.Add(new CommandBinding(FileManagerCommands.ExitCommand, ExecuteExitCommand));

            LayoutRoot.PreviewKeyDown += LayoutRootOnPreviewKeyDown;
            Closing += OnClosing;

            eventAggregator.GetEvent<CacheMigrationEvent>().Subscribe(OnCacheMigration);
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            ViewModel.Dispose();
        }

        //HACK: Temporary solution. KeyBinding doesn't work with Key.Delete (requires investigation)
        private void LayoutRootOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            var deleteCommand = ViewModel.DeleteCommand;
            if (!deleteCommand.CanExecute()) return;
            e.Handled = true;
            deleteCommand.Execute();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source == null) return;
            source.AddHook(HwndHandler);
            UsbNotification.RegisterUsbDeviceNotification(source.Handle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == UsbNotification.WmDevicechange)
            {
                var e = (UsbDeviceChange) wparam;
                if (Enum.IsDefined(typeof(UsbDeviceChange), e)) 
                    _eventAggregator.GetEvent<UsbDeviceChangedEvent>().Publish(new UsbDeviceChangedEventArgs(e));
            }
            return IntPtr.Zero;
        }

        private void ExecuteOpenDriveDropdownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var control = e.Parameter as ContentControl;
            if (control == null) return;
            var pane = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(control, 0), 0) as FileListPane;
            if (pane == null) return;
            var combobox = pane.FindName("DriveDropdown") as ComboBox;
            if (combobox == null) return;

            combobox.IsDropDownOpen = true;
            var item = combobox.ItemContainerGenerator.ContainerFromItem(combobox.SelectedItem) as ComboBoxItem;
            item.Focus();
        }

        private void ExecuteSettingsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var settings = UnityInstance.Container.Resolve<SettingsWindow>();
            settings.ShowDialog();
        }

        private void ExecuteStatisticsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var statistics = UnityInstance.Container.Resolve<StatisticsWindow>();
            statistics.ShowDialog();
        }

        private void ExecuteAboutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void ExecuteVisitWebsiteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Web.Browse(e.Parameter.ToString());
        }

        private void ExecuteUserStatisticsParticipationCommand(object sender, ExecutedRoutedEventArgs e)
        {
            new UserStatisticsParticipationDialog().ShowDialog();
        }

        private void ExecuteExitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ViewModelOnTransferStarted()
        {
            if (_transferProgressDialog == null)
            {
                _transferProgressDialog = new TransferProgressDialog(ViewModel);
                _transferProgressDialog.Closing += TransferProgressDialogOnClosing;
                _transferProgressDialog.Closed += TransferProgressDialogOnClosed;
            }
            IsHitTestVisible = false;
            _isAbortionInProgress = false;
            _transferProgressDialog.Show();
        }

        private void TransferProgressDialogOnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            if (_isAbortionInProgress) return;
            if (!ViewModel.TargetPane.IsResumeSupported)
            {
                var d = new ConfirmationDialog(Resx.Warning, Resx.ResumeIsNotAvailableConfirmation);
                if (d.ShowDialog() != true) return;
            }
            _transferProgressDialog.Abort.IsEnabled = false;
            _isAbortionInProgress = true;
            ViewModel.AbortTransfer();
        }

        private void TransferProgressDialogOnClosed(object sender, EventArgs e)
        {
            IsHitTestVisible = true;
            _transferProgressDialog.Closed -= TransferProgressDialogOnClosed;
            _transferProgressDialog = null;
        }

        private void ViewModelOnTransferFinished()
        {
            if (_transferProgressDialog == null) return;
            _transferProgressDialog.Closing -= TransferProgressDialogOnClosing;
            _transferProgressDialog.Close();
        }

        private void OnCacheMigration(CacheMigrationEventArgs e)
        {
            IsHitTestVisible = false;
            _migrationProgressDialog = UnityInstance.Container.Resolve<MigrationProgressDialog>();
            var vm = (CacheMigrationViewModel) _migrationProgressDialog.DataContext;
            vm.MigrationFinished += OnMigrationFinished;
            vm.Initialize(e);
            _migrationProgressDialog.Show();
        }

        private void OnMigrationFinished(CacheMigrationViewModel sender)
        {
            sender.MigrationFinished -= OnMigrationFinished;
            IsHitTestVisible = true;
            _migrationProgressDialog.Close();
        }

        private void OnUserMessagesOpened(object sender, RoutedEventArgs e)
        {
            _userMessageReadTimers = new Queue<Timer>();
            EnqueueNewTimer();
            ((MenuItem)sender).ItemContainerGenerator.ItemsChanged += OnUserMessagesItemsChanged;
        }

        private void EnqueueNewTimer()
        {
            var items = ViewModel.UserMessages.Where(m => !m.IsRead).ToArray();
            if (items.Length == 0) return;
            var timer = new Timer(CheckUserMessages, items, 3000, -1);
            _userMessageReadTimers.Enqueue(timer);
        }

        private void OnUserMessagesClosed(object sender, RoutedEventArgs e)
        {
            ((MenuItem)sender).ItemContainerGenerator.ItemsChanged -= OnUserMessagesItemsChanged;
            lock (_userMessageReadTimers)
            {
                while (_userMessageReadTimers.Count > 0)
                {
                    var timer = _userMessageReadTimers.Dequeue();
                    timer.Dispose();
                }
            }
        }

        private void OnUserMessagesItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            EnqueueNewTimer();
        }

        private void CheckUserMessages(object state)
        {
            UIThread.Run(() =>
                             {
                                 ViewModel.SetUserMessagesToRead((IUserMessageViewModel[])state);
                                 _userMessageReadTimers.Dequeue();
                             });
        }

    }
}