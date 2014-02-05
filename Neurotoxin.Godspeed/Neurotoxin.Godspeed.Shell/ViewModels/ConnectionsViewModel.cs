﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class ConnectionsViewModel : PaneViewModelBase
    {
        private IStoredConnectionViewModel _previouslyFocusedItem;
        public const string CacheStoreKeyPrefix = "FtpConnection_";
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

        #region Properties

        public FtpContentViewModel ConnectedFtp { get; private set; }

        private const string ITEMS = "Items";
        private ObservableCollection<IStoredConnectionViewModel> _items;
        public ObservableCollection<IStoredConnectionViewModel> Items
        {
            get { return _items; }
            private set { _items = value; NotifyPropertyChanged(ITEMS); }
        }

        private const string SELECTEDITEM = "SelectedItem";
        private IStoredConnectionViewModel _selectedItem;
        public IStoredConnectionViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; NotifyPropertyChanged(SELECTEDITEM); }
        }

        #endregion

        #region ConnectCommand

        public DelegateCommand<object> ConnectCommand { get; private set; }

        private bool CanExecuteConnectCommand(object cmdParam)
        {
            var mouseEvent = cmdParam as EventInformation<MouseEventArgs>;
            if (mouseEvent != null)
            {
                var e = mouseEvent.EventArgs;
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                if (!(dataContext is IStoredConnectionViewModel)) return false;
            }

            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null)
            {
                var e = keyEvent.EventArgs;
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                if (!(dataContext is IStoredConnectionViewModel)) return false;
                return e.Key == Key.Enter;
            }

            return SelectedItem != null;
        }

        private void ExecuteConnectCommand(object cmdParam)
        {
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null) keyEvent.EventArgs.Handled = true;

            if (SelectedItem is NewConnectionPlaceholderViewModel)
            {
                Edit();
            }
            else if (SelectedItem is FtpConnectionItemViewModel)
            {
                FtpConnect(SelectedItem);
            }
        }

        #endregion

        public ConnectionsViewModel(FileManagerViewModel parent) : base(parent)
        {
            ConnectCommand = new DelegateCommand<object>(ExecuteConnectCommand, CanExecuteConnectCommand);
            Items = new ObservableCollection<IStoredConnectionViewModel>();
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    foreach (var ftpconn in _cacheStore.Keys.Where(key => key.StartsWith(CacheStoreKeyPrefix))
                                                            .Select(key => _cacheStore.Get<FtpConnection>(key))
                                                            .OrderBy(i => i.Name))
                    {
                        Items.Add(new FtpConnectionItemViewModel(ftpconn));
                    }
                    var add = new NewConnectionPlaceholderViewModel();
                    Items.Add(add);
                    break;
                case LoadCommand.Restore:
                    Save(cmdParam as FtpConnectionItemViewModel);
                    break;
            }
            if (success != null) success.Invoke(this);
        }

        public override void SetActive()
        {
            base.SetActive();
            SelectedItem = SelectedItem ?? _previouslyFocusedItem ?? Items.FirstOrDefault();
        }

        protected override void OnActivePaneChanged(Events.ActivePaneChangedEventArgs e)
        {
            base.OnActivePaneChanged(e);
            if (e.ActivePane == this) return;
            _previouslyFocusedItem = SelectedItem;
            SelectedItem = null;
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            Parent.EditCommand.RaiseCanExecuteChanged();
        }

        public void Edit()
        {
            bool edit;
            FtpConnectionItemViewModel newItem;
            var ftpconn = SelectedItem as FtpConnectionItemViewModel;
            if (ftpconn != null)
            {
                edit = true;
                newItem = ftpconn.Clone();
            } 
            else if (SelectedItem is NewConnectionPlaceholderViewModel)
            {
                edit = false;
                newItem = new FtpConnectionItemViewModel(new FtpConnection
                    {
                        Port = 21,
                        Username = "xbox",
                        Password = "xbox",
                        ConnectionImage = (int)ConnectionImage.Fat,
                    });
            }
            else
            {
                throw new NotSupportedException();
            }

            var dialog = new NewConnectionDialog(newItem);
            if (dialog.ShowDialog() != true) return;

            if (edit)
            {
                Items.Remove(SelectedItem);
                _cacheStore.Remove(string.Format("{0}{1}", CacheStoreKeyPrefix, ftpconn.Name));
            }
            var i = 0;
            while (i < Items.Count - 1 && String.Compare(newItem.Name, Items[i].Name, StringComparison.InvariantCultureIgnoreCase) == 1) i++;
            Items.Insert(i, newItem);
            _cacheStore.Put(string.Format("{0}{1}", CacheStoreKeyPrefix, newItem.Name), newItem.Model);
            SelectedItem = newItem;
        }

        public void Delete()
        {
            var ftpconn = SelectedItem as FtpConnectionItemViewModel;
            if (ftpconn == null) return;
            Items.Remove(SelectedItem);
            _cacheStore.Remove(string.Format("{0}{1}", CacheStoreKeyPrefix, ftpconn.Name));
        }

        private void FtpConnect(IStoredConnectionViewModel connection)
        {
            IsBusy = true;
            ProgressMessage = string.Format("Connecting to {0}...", connection.Name);
            ConnectedFtp = container.Resolve<FtpContentViewModel>();
            var dir = Settings.Directory.StartsWith(connection.Name)
                          ? Settings.Directory.Replace(string.Format("{0}:/", connection.Name), string.Empty)
                          : "/Hdd1";
            ConnectedFtp.LoadDataAsync(LoadCommand.Load, new Tuple<IStoredConnectionViewModel, FileListPaneSettings>(connection, new FileListPaneSettings(dir, Settings.SortByField, Settings.SortDirection)), FtpConnectSuccess, FtpConnectError);
        }

        private void FtpConnectSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            eventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void FtpConnectError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            var connectionName = ConnectedFtp.Connection.Name;
            if (exception is EstablishmentFailedException)
            {
                ErrorMessage.Show(exception);
            } 
            else
            {
                NotificationMessage.ShowMessage("Connection failed", string.Format("Can't connect to {0}", connectionName));
            }
            ConnectedFtp = null;
        }

        private void Save(FtpConnectionItemViewModel connection)
        {
            if (connection == null) return;
            _cacheStore.Update(string.Format("{0}{1}", CacheStoreKeyPrefix, connection.Name), connection.Model);
        }

        public override object Close(object data)
        {
            Save(data as FtpConnectionItemViewModel);
            return base.Close(data);
        }
    }
}