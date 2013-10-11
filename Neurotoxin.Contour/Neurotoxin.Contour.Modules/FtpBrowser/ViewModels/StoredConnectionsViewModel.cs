using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using System.Linq;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class StoredConnectionsViewModel : PaneViewModelBase, IPaneViewModel
    {
        private bool _isBusy;

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
            set { _selectedItem = value; NotifyPropertyChanged(SELECTEDITEM); RaiseCanExecuteChanges(); }
        }

        #endregion

        #region ConnectCommand

        public DelegateCommand<object> ConnectCommand { get; private set; }

        private bool CanExecuteConnectCommand(object cmdParam)
        {
            if (_isBusy) return false;

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
            _isBusy = true;
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null) keyEvent.EventArgs.Handled = true;

            if (SelectedItem is NewConnectionPlaceholderViewModel)
            {
                throw new NotImplementedException();
            }
            else if (SelectedItem is FtpConnectionItemViewModel)
            {
                ((FtpBrowserViewModel)Parent).FtpConnect(SelectedItem);
            }
        }

        #endregion


        public StoredConnectionsViewModel(ModuleViewModelBase parent) : base(parent)
        {
            ConnectCommand = new DelegateCommand<object>(ExecuteConnectCommand, CanExecuteConnectCommand);
            Items = new ObservableCollection<IStoredConnectionViewModel>();
        }

        public void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    var items = new List<FtpConnection>
                        {
                            new FtpConnection
                                {
                                    Name = "Localhost (test)",
                                    Address = "127.0.0.1",
                                    Port = 21,
                                    Username = "xbox",
                                    Password = "hardcore21*",
                                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/Connections/fat_elite.png")
                                },
                            new FtpConnection
                                {
                                    Name = "Ivory",
                                    Address = "192.168.1.110",
                                    Port = 21,
                                    Username = "xbox",
                                    Password = "xbox",
                                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/Connections/fat.png")
                                },
                            new FtpConnection
                                {
                                    Name = "Ebony",
                                    Address = "192.168.1.111",
                                    Port = 21,
                                    Username = "xbox",
                                    Password = "xbox",
                                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/Connections/slim.png")
                                }
                        };
                    foreach (var item in items.OrderBy(i => i.Name))
                    {
                        Items.Add(new FtpConnectionItemViewModel(item));
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
    }
}