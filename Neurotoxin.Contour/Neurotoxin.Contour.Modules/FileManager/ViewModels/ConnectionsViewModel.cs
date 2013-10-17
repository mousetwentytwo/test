using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Contour.Core.Caching;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Modules.FileManager.Views.Dialogs;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public class ConnectionsViewModel : PaneViewModelBase
    {
        private IStoredConnectionViewModel _previouslyFocusedItem;
        private const string CacheStoreKeyPrefix = "FtpConnection_";
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

        #region Properties

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
                Parent.FtpConnect(SelectedItem);
            }
        }

        #endregion

        public ConnectionsViewModel(FileManagerViewModel parent) : base(parent)
        {
            ConnectCommand = new DelegateCommand<object>(ExecuteConnectCommand, CanExecuteConnectCommand);
            Items = new ObservableCollection<IStoredConnectionViewModel>();
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase> error = null)
        {
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
            }
        }

        public override void Refresh()
        {
            throw new NotImplementedException();
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
    }
}