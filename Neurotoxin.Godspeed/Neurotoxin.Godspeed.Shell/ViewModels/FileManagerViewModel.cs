using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{

    public class FileManagerViewModel : ViewModelBase, IFileManagerViewModel
    {
        private readonly TransferManagerViewModel _transferManager;
        private readonly IUserSettings _userSettings;

        #region Main window properties

        private readonly Stack<IPaneViewModel> _leftPaneStack = new Stack<IPaneViewModel>();

        private const string LEFTPANE = "LeftPane";
        private IPaneViewModel _leftPane;
        public IPaneViewModel LeftPane
        {
            get { return _leftPane; }
            set { _leftPane = value; NotifyPropertyChanged(LEFTPANE); }
        }

        private readonly Stack<IPaneViewModel> _rightPaneStack = new Stack<IPaneViewModel>();

        private const string RIGHTPANE = "RightPane";
        private IPaneViewModel _rightPane;
        public IPaneViewModel RightPane
        {
            get { return _rightPane; }
            set { _rightPane = value; NotifyPropertyChanged(RIGHTPANE); }
        }

        private const string ACTIVEPANE = "ActivePane";
        public IPaneViewModel ActivePane
        {
            get 
            { 
                if (LeftPane != null && LeftPane.IsActive) return LeftPane;
                if (RightPane != null && RightPane.IsActive) return RightPane;
                return null;
            }
        }

        private const string OTHERPANE = "OtherPane";
        public IPaneViewModel OtherPane
        {
            get { return LeftPane.IsActive ? RightPane : LeftPane; }
        }

        private const string SOURCEPANE = "SourcePane";
        public IFileListPaneViewModel SourcePane
        {
            get
            {
                var left = LeftPane as IFileListPaneViewModel;
                var right = RightPane as IFileListPaneViewModel;
                if (left != null && left.IsActive) return left;
                if (right != null && right.IsActive) return right;
                return null;
            }
        }

        private const string TARGETPANE = "TargetPane";
        public IFileListPaneViewModel TargetPane
        {
            get
            {
                var left = LeftPane as IFileListPaneViewModel;
                var right = RightPane as IFileListPaneViewModel;
                if (left != null && !left.IsActive) return left;
                if (right != null && !right.IsActive) return right;
                return null;
            }
        }

        private FtpContentViewModel Ftp
        {
            get
            {
                return SourcePane as FtpContentViewModel ?? TargetPane as FtpContentViewModel;
            }
        }

        private const string UNREADMESSAGECOUNT = "UnreadMessageCount";
        public int UnreadMessageCount
        {
            get { return UserMessages.Count(m => !m.IsRead); }
        }

        public ObservableCollection<IUserMessageViewModel> UserMessages { get; private set; }

        #endregion

        #region SwitchPaneCommand

        public DelegateCommand<EventInformation<KeyEventArgs>> SwitchPaneCommand { get; private set; }

        private bool CanExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            return eventInformation.EventArgs.Key == Key.Tab;
        }

        private void ExecuteSwitchPaneCommand(EventInformation<KeyEventArgs> eventInformation)
        {
            OtherPane.SetActive();
            eventInformation.EventArgs.Handled = true;
        }

        #endregion

        #region EditCommand

        public DelegateCommand EditCommand { get; private set; }

        private bool CanExecuteEditCommand()
        {
            var pane = ActivePane as ConnectionsViewModel;
            return pane != null && pane.SelectedItem is FtpConnectionItemViewModel && !pane.IsBusy;
        }
        
        private void ExecuteEditCommand()
        {
            ((ConnectionsViewModel)ActivePane).Edit();
        }

        #endregion

        #region CopyCommand

        public DelegateCommand CopyCommand { get; private set; }

        private bool CanExecuteCopyCommand()
        {
            return TargetPane != null && SourcePane != null && SourcePane.HasValidSelection && !SourcePane.IsBusy &&
                   !TargetPane.IsBusy && !TargetPane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteCopyCommand()
        {
            if (!ConfirmCommand(FileOperation.Copy, SourcePane.SelectedItems.Count() > 1)) return;
            _transferManager.Copy(SourcePane, TargetPane);
        }

        #endregion

        #region MoveCommand

        public DelegateCommand MoveCommand { get; private set; }

        private bool CanExecuteMoveCommand()
        {
            return TargetPane != null && SourcePane != null && SourcePane.HasValidSelection && !SourcePane.IsBusy &&
                   !TargetPane.IsBusy && !TargetPane.IsReadOnly && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteMoveCommand()
        {
            if (!ConfirmCommand(FileOperation.Move, SourcePane.SelectedItems.Count() > 1)) return;
            _transferManager.Move(SourcePane, TargetPane);
        }

        #endregion

        #region NewFolderCommand

        public DelegateCommand NewFolderCommand { get; private set; }

        private bool CanExecuteNewFolderCommand()
        {
            return SourcePane != null && !SourcePane.IsBusy && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteNewFolderCommand()
        {
            var items = SourcePane.Items.Select(item => item.Name).ToList();
            var wkDirs = DirectoryStructure.WellKnownDirectoriesOf(SourcePane.CurrentFolder.Path);
            var suggestion = wkDirs.Where(d => !items.Contains(d)).Select(d => new InputDialogOptionViewModel
                {
                    Value = d,
                    DisplayName = TitleRecognizer.GetTitle(d)
                }).ToList();

            var name = InputDialog.ShowText(Resx.AddNewFolder, Resx.FolderName + Strings.Colon, string.Empty, suggestion);
            if (name == null) return;
            if (name == string.Empty)
            {
                NotificationMessage.ShowMessage(Resx.AddNewFolder, Resx.CannotCreateFolderWithNoName);
                return;
            }
            var invalidChars = Path.GetInvalidFileNameChars();
            if (invalidChars.Any(name.Contains))
            {
                NotificationMessage.ShowMessage(Resx.AddNewFolder, Resx.CannotCreateFolderWithInvalidCharacters);
                return;
            }
            var path = string.Format("{0}{1}", SourcePane.CurrentFolder.Path, name);
            WorkerThread.Run(() => SourcePane.CreateFolder(path), result => NewFolderSuccess(result, name), NewFolderError);
        }

        private void NewFolderSuccess(TransferResult result, string name)
        {
            if (result != TransferResult.Ok)
            {
                NotificationMessage.ShowMessage(Resx.AddNewFolder, string.Format(Resx.FolderAlreadyExists, name));
                return;
            }
            SourcePane.Refresh(() =>
                                   {
                                       SourcePane.CurrentRow = SourcePane.Items.Single(item => item.Name == name);
                                   });
        }

        private void NewFolderError(Exception ex)
        {
            eventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Publish(new ShowCorrespondingErrorEventArgs(ex, false));
        }

        #endregion

        #region DeleteCommand

        public DelegateCommand DeleteCommand { get; private set; }

        private bool CanExecuteDeleteCommand()
        {
            var connections = ActivePane as ConnectionsViewModel;
            if (connections != null)
            {
                return connections.SelectedItem != null && !connections.IsBusy;
            }
            return SourcePane != null && SourcePane.HasValidSelection && !SourcePane.IsBusy && !SourcePane.IsReadOnly && !SourcePane.IsInEditMode;
        }

        private void ExecuteDeleteCommand()
        {
            var connections = ActivePane as ConnectionsViewModel;
            if (!ConfirmCommand(FileOperation.Delete, connections == null && SourcePane.SelectedItems.Count() > 1)) return;
            if (connections != null)
            {
                connections.Delete();
            }
            else
            {
                _transferManager.Delete(SourcePane);
            }
        }


        #endregion

        #region PauseCommand

        public DelegateCommand PauseCommand { get; private set; }

        private void ExecutePauseCommand()
        {
            _transferManager.Pause();
        }

        #endregion

        #region ContinueCommand

        public DelegateCommand ContinueCommand { get; private set; }

        private void ExecuteContinueCommand()
        {
            _transferManager.Continue();
        }

        #endregion

        #region OpenUserMessageCommand

        public DelegateCommand<UserMessageCommandParameter> OpenUserMessageCommand { get; private set; }

        private void ExecuteOpenUserMessageCommand(UserMessageCommandParameter p)
        {
            var message = p.ViewModel;
            message.IsRead = true;
            message.IsChecked = true;
            NotifyPropertyChanged(UNREADMESSAGECOUNT);

            switch (p.Command)
            {
                case MessageCommand.OpenUrl:
                    if (Web.Browse((string)p.Parameter))
                    {
                        if (message.Flags.HasFlag(MessageFlags.IgnoreAfterOpen)) _userSettings.IgnoreMessage(message.Message);
                    }
                    break;
                case MessageCommand.OpenDialog:
                    var dialogType = (Type) p.Parameter;
                    var c = dialogType.GetConstructor(Type.EmptyTypes);
                    var d = c.Invoke(new object[0]) as Window;
                    if (d.ShowDialog() == true) _userSettings.IgnoreMessage(message.Message);
                    break;
            }
        }

        #endregion

        #region RemoveUserMessageCommand

        public DelegateCommand<UserMessageViewModel> RemoveUserMessageCommand { get; private set; }

        private void ExecuteRemoveUserMessageCommand(UserMessageViewModel message)
        {
            if (message.Flags.HasFlag(MessageFlags.Ignorable) && 
                new ConfirmationDialog(Resx.RemoveUserMessage, Resx.RemoveUserMessageConfirmation).ShowDialog() == true)
            {
                _userSettings.IgnoreMessage(message.Message);
            }
            UserMessages.Remove(message);
            if (UserMessages.Count == 0) UserMessages.Add(new NoMessagesViewModel());
        }

        #endregion

        public FileManagerViewModel(TransferManagerViewModel transferManager, IUserSettings userSettings)
        {
            _transferManager = transferManager;
            _userSettings = userSettings;
            UserMessages = new ObservableCollection<IUserMessageViewModel> { new NoMessagesViewModel() };
            UserMessages.CollectionChanged += (sender, args) => NotifyPropertyChanged(UNREADMESSAGECOUNT);

            SwitchPaneCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSwitchPaneCommand, CanExecuteSwitchPaneCommand);
            EditCommand = new DelegateCommand(ExecuteEditCommand, CanExecuteEditCommand);
            CopyCommand = new DelegateCommand(ExecuteCopyCommand, CanExecuteCopyCommand);
            MoveCommand = new DelegateCommand(ExecuteMoveCommand, CanExecuteMoveCommand);
            NewFolderCommand = new DelegateCommand(ExecuteNewFolderCommand, CanExecuteNewFolderCommand);
            DeleteCommand = new DelegateCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            PauseCommand = new DelegateCommand(ExecutePauseCommand);
            ContinueCommand = new DelegateCommand(ExecuteContinueCommand);
            OpenUserMessageCommand = new DelegateCommand<UserMessageCommandParameter>(ExecuteOpenUserMessageCommand);
            RemoveUserMessageCommand = new DelegateCommand<UserMessageViewModel>(ExecuteRemoveUserMessageCommand);

            eventAggregator.GetEvent<OpenNestedPaneEvent>().Subscribe(OnOpenNestedPane);
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Subscribe(OnCloseNestedPane);
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(OnActivePaneChanged);
            eventAggregator.GetEvent<NotifyUserMessageEvent>().Subscribe(OnNotifyUserMessage);
        }

        public void Initialize()
        {
            LeftPane = (IPaneViewModel)container.Resolve(GetStoredPaneType(_userSettings.LeftPaneType));
            var leftParam = _userSettings.LeftPaneFileListPaneSettings;
            LeftPane.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(leftParam), PaneLoaded);

            RightPane = (IPaneViewModel)container.Resolve(GetStoredPaneType(_userSettings.RightPaneType));
            var rightParam = _userSettings.RightPaneFileListPaneSettings;
            RightPane.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(rightParam), PaneLoaded);
        }

        private void PaneLoaded(PaneViewModelBase pane)
        {
            if (!LeftPane.IsLoaded || !RightPane.IsLoaded) return;
            eventAggregator.GetEvent<ShellInitializedEvent>().Publish(new ShellInitializedEventArgs());
        }

        private static Type GetStoredPaneType(string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType(typeName);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            EditCommand.RaiseCanExecuteChanged();
            CopyCommand.RaiseCanExecuteChanged();
            MoveCommand.RaiseCanExecuteChanged();
            NewFolderCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }

        public void SetUserMessagesToRead(IUserMessageViewModel[] items)
        {
            items.ForEach(item => item.IsRead = true);
            NotifyPropertyChanged(UNREADMESSAGECOUNT);
        }

        private void OnOpenNestedPane(OpenNestedPaneEventArgs args)
        {
            if (LeftPane == args.Opener)
            {
                _leftPaneStack.Push(LeftPane);
                LeftPane = args.Openee;
            }
            else if (RightPane == args.Opener)
            {
                _rightPaneStack.Push(RightPane);
                RightPane = args.Openee;
            }
        }

        private void OnCloseNestedPane(CloseNestedPaneEventArgs args)
        {
            if (LeftPane == args.Pane)
            {
                var settings = LeftPane.Settings;
                LeftPane.Dispose();
                LeftPane = _leftPaneStack.Pop();
                LeftPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings, args.Payload));
                LeftPane.SetActive();
            }
            else if (RightPane == args.Pane)
            {
                var settings = LeftPane.Settings;
                RightPane.Dispose();
                RightPane = _rightPaneStack.Pop();
                RightPane.LoadDataAsync(LoadCommand.Restore, new LoadDataAsyncParameters(settings, args.Payload));
                RightPane.SetActive();
            }
        }

        private void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            NotifyPropertyChanged(ACTIVEPANE);
        }

        private void OnNotifyUserMessage(NotifyUserMessageEventArgs e)
        {
            if (!UIThread.IsUIThread)
            {
                UIThread.Run(() => OnNotifyUserMessage(e));
                return;
            }
            if (_userSettings.IsMessageIgnored(e.MessageKey)) return;
            var message = string.Format(Resx.ResourceManager.GetString(e.MessageKey), e.MessageArgs);
            var i = UserMessages.IndexOf(m => m.Message == message);
            if (i == -1)
            {
                if (UserMessages.First() is NoMessagesViewModel) UserMessages.RemoveAt(0);
                UserMessages.Insert(0, new UserMessageViewModel(message, e));
            } 
            else if (i != 0)
            {
                UserMessages.Move(i, 0);
            }
        }

        private bool ConfirmCommand(FileOperation type, bool isPlural)
        {
            var title = Resx.ResourceManager.GetString(type.ToString());
            var message = Resx.ResourceManager.GetString(string.Format(isPlural ? "{0}ConfirmationPlural" : "{0}ConfirmationSingular", type));
            if (new ConfirmationDialog(title, message).ShowDialog() != true) return false;
            //TODO: introduce separate method for the following
            if (Ftp != null) Ftp.IsKeepAliveEnabled = true;
            return true;
        }

        public override void Dispose()
        {
            object data = null;
            IPaneViewModel left;
            do
            {
                data = LeftPane.Close(data);
                left = LeftPane;
                LeftPane = _leftPaneStack.Count > 0 ? _leftPaneStack.Pop() : null;
            } 
            while (LeftPane != null);  
            _userSettings.LeftPaneType = left.GetType().FullName;
            _userSettings.LeftPaneFileListPaneSettings = left.Settings;

            data = null;
            IPaneViewModel right;
            do
            {
                data = RightPane.Close(data);
                right = RightPane;
                RightPane = _rightPaneStack.Count > 0 ? _rightPaneStack.Pop() : null;
            }
            while (RightPane != null);
            _userSettings.RightPaneType = right.GetType().FullName;
            _userSettings.RightPaneFileListPaneSettings = right.Settings;

            base.Dispose();
        }
    }
}