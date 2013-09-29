using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using Expression = System.Linq.Expressions.Expression;
using Microsoft.Practices.Composite;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public abstract class PaneViewModelBase : ViewModelBase
    {
        protected readonly BinaryFormatter BinaryFormatter;

        protected ModuleViewModelBase Parent { get; set; }

        #region Properties

        private const string ISACTIVE = "IsActive";
        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value; 
                NotifyPropertyChanged(ISACTIVE);
                if (value) eventAggregator.GetEvent<ActivePaneChangedEvent>().Publish(new ActivePaneChangedEventArgs(this));
            }
        }

        private const string DRIVE = "Drive";
        private string _drive;
        public string Drive
        {
            get { return _drive; }
            set { _drive = value; NotifyPropertyChanged(DRIVE); }
        }

        private const string STACK = "Stack";
        private Stack<FileSystemItem> _stack;
        public Stack<FileSystemItem> Stack
        {
            get { return _stack; }
            set { _stack = value; NotifyPropertyChanged(STACK); }
        }

        private const string CURRENTFOLDER = "CurrentFolder";
        public FileSystemItemViewModel CurrentFolder
        {
            get { return _stack != null && _stack.Count > 0 ? new FileSystemItemViewModel(_stack.Peek()) : null; }
        }

        private const string ITEMS = "Items";
        private ObservableCollection<FileSystemItemViewModel> _items;
        public ObservableCollection<FileSystemItemViewModel> Items
        {
            get { return _items; }
            set { _items = value; NotifyPropertyChanged(ITEMS); }
        }

        private const string CURRENTROW = "CurrentRow";
        private FileSystemItemViewModel _currentRow;
        public FileSystemItemViewModel CurrentRow
        {
            get { return _currentRow; }
            set
            {
                _currentRow = value; 
                NotifyPropertyChanged(CURRENTROW);
                if (value == null) return;
                var cell = FocusManager.GetFocusedElement(Application.Current.Windows[0]) as DataGridCell;
                if (cell != null && cell.DataContext != value)
                {
                    var grid = cell.FindAncestor<DataGrid>();
                    if (grid != null)
                    {
                        var rowContainer = grid.ItemContainerGenerator.ContainerFromItem(value) as DataGridRow;
                        if (rowContainer != null)
                        {
                            var root = VisualTreeHelper.GetChild(rowContainer, 0);
                            var cellsPresenter = (DataGridCellsPresenter)VisualTreeHelper.GetChild(root, 0);
                            var firstCell = cellsPresenter.ItemContainerGenerator.ContainerFromIndex(0) as DataGridCell;
                            if (firstCell != null) firstCell.Focus();
                        }
                    }
                }
            }
        }

        private string _sortMemberPath;
        private ListSortDirection _listSortDirection;

        public FileSystemItemViewModel[] SelectedItems
        {
            get { return Items.Where(item => item.IsSelected).ToArray(); }
        }

        #endregion

        #region SetActiveCommand

        public DelegateCommand<EventInformation<MouseEventArgs>> SetActiveCommand { get; private set; }

        private void ExecuteSetActiveCommand(EventInformation<MouseEventArgs> eventInformation)
        {
            IsActive = true;
        }

        #endregion

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            var eventInformation = cmdParam as EventInformation<System.Windows.Input.MouseEventArgs>;
            if (eventInformation != null)
            {
                var e = eventInformation.EventArgs;
                var dataContext = ((FrameworkElement) e.OriginalSource).DataContext;
                if (!(dataContext is FileSystemItemViewModel) || CurrentRow.Type == ItemType.File) return;
            }

            if (CurrentRow != null)
            {
                if (CurrentRow.IsUpDirectory)
                    Stack.Pop();
                else
                    Stack.Push(CurrentRow.Model);
            }
            NotifyPropertyChanged(CURRENTFOLDER);
            Parent.IsInProgress = true;
            WorkerThread.Run(ChangeDirectory, ChangeDirectoryCallback);
        }

        protected abstract List<FileSystemItem> ChangeDirectory();

        protected virtual void ChangeDirectoryCallback(List<FileSystemItem> items)
        {
            SortContent(items.Select(c => new FileSystemItemViewModel(c)));
            if (IsActive) CurrentRow = Items.FirstOrDefault();
            Parent.IsInProgress = false;
        }

        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<bool> CalculateSizeCommand { get; private set; }
        private Queue<FileSystemItemViewModel> _calculationQueue;

        private void ExecuteCalculateSizeCommand(bool calculateAll)
        {
            Parent.IsInProgress = true;
            if (calculateAll)
            {
                _calculationQueue = new Queue<FileSystemItemViewModel>(SelectedItems);
            } 
            else
            {
                _calculationQueue = new Queue<FileSystemItemViewModel>();
                _calculationQueue.Enqueue(CurrentRow);
            }
            if (_calculationQueue.Count > 0) WorkerThread.Run(CalculateSize, CalculateSizeCallback);
        }

        private bool CanExecuteCalculateSizeCommand(bool cmdParam)
        {
            return CurrentRow != null;
        }

        private long CalculateSize()
        {
            return CalculateSize(_calculationQueue.Peek().Path);
        }

        protected abstract long CalculateSize(string path);

        protected virtual void CalculateSizeCallback(long size)
        {
            _calculationQueue.Dequeue().Size = size;
            if (_calculationQueue.Count > 0)
            {
                WorkerThread.Run(CalculateSize, CalculateSizeCallback);
            } 
            else
            {
                Parent.IsInProgress = false;
            }
        }

        #endregion

        #region SortingCommand

        public DelegateCommand<EventInformation<DataGridSortingEventArgs>> SortingCommand { get; private set; }

        private void ExecuteSortingCommand(EventInformation<DataGridSortingEventArgs> cmdParam)
        {
            var e = cmdParam.EventArgs;
            var column = e.Column;
            e.Handled = true;
            _sortMemberPath = column.SortMemberPath;
            _listSortDirection = column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            var selection = CurrentRow;
            SortContent(Items);
            CurrentRow = selection;
            column.SortDirection = _listSortDirection;
        }

        private void SortContent(IEnumerable<FileSystemItemViewModel> content)
        {
            Items = Items ?? new ObservableCollection<FileSystemItemViewModel>();
            if (_sortMemberPath == null)
            {
                Items.Clear();
                Items.AddRange(content);
                return;
            }
            var type = typeof(FileSystemItemViewModel);
            var instance = Expression.Parameter(type);
            var callPreporty = Expression.PropertyOrField(instance, _sortMemberPath);
            var lambda = Expression.Lambda<Func<FileSystemItemViewModel, object>>(callPreporty, instance);
            var orderBy = lambda.Compile();

            var collection = _listSortDirection == ListSortDirection.Ascending
                                 ? content.OrderBy(orderBy)
                                 : content.OrderByDescending(orderBy);

            if (_sortMemberPath == "Title")
                collection = _listSortDirection == ListSortDirection.Ascending
                                 ? collection.ThenBy(p => p.TitleId)
                                 : collection.ThenByDescending(p => p.TitleId);

            collection = collection.ThenByDescending(p => p.Type);
            var list = collection.ToList();

            var up = list.FirstOrDefault(item => item.IsUpDirectory);
            if (up != null)
            {
                list.Remove(up);
                list.Insert(0, up);
            }

            Items.Clear();
            Items.AddRange(list);
        }

        #endregion

        #region ToggleSelectionCommand

        public DelegateCommand<ToggleSelectionMode> ToggleSelectionCommand { get; private set; }

        private void ExecuteToggleSelectionCommand(ToggleSelectionMode mode)
        {
            CurrentRow.IsSelected = !CurrentRow.IsSelected;
            switch (mode)
            {
                case ToggleSelectionMode.Space:
                    if (CurrentRow.IsSelected) CalculateSizeCommand.Execute(false);    
                    break;
                case ToggleSelectionMode.Insert:
                case ToggleSelectionMode.ShiftDown:
                    {
                        var index = Items.IndexOf(CurrentRow);
                        if (index < Items.Count - 1) CurrentRow = Items[index + 1];
                    }
                    break;
                case ToggleSelectionMode.ShiftUp:
                    {
                        var index = Items.IndexOf(CurrentRow);
                        if (index > 0) CurrentRow = Items[index - 1];
                    }
                    break;
            }
        }

        #endregion

        #region SelectAllCommand

        public DelegateCommand<EventInformation<EventArgs>> SelectAllCommand { get; private set; }

        private void ExecuteSelectAllCommand(EventInformation<EventArgs> cmdParam)
        {
            Items.Where(row => !row.IsUpDirectory).ForEach(row => row.IsSelected = true);
        }

        #endregion

        #region MouseSelectionCommand

        public DelegateCommand<EventInformation<MouseEventArgs>> MouseSelectionCommand { get; private set; }

        private void ExecuteMouseSelectionCommand(EventInformation<MouseEventArgs> eventInformation)
        {
            var e = eventInformation.EventArgs;
            var item = ((FrameworkElement)e.OriginalSource).DataContext as FileSystemItemViewModel;
            if (item == null) return;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var start = Items.IndexOf(CurrentRow);
                var end = Items.IndexOf(item);
                if (end < start)
                {
                    var tmp = start;
                    start = end;
                    end = tmp;
                }
                for (var i = start; i <= end; i++)
                {
                    Items[i].IsSelected = true;
                }
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                item.IsSelected = true;
            }
        }

        #endregion

        protected PaneViewModelBase(ModuleViewModelBase parent)
        {
            Parent = parent;
            BinaryFormatter = new BinaryFormatter();
            SetActiveCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteSetActiveCommand);
            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand);
            CalculateSizeCommand = new DelegateCommand<bool>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            ToggleSelectionCommand = new DelegateCommand<ToggleSelectionMode>(ExecuteToggleSelectionCommand);
            SelectAllCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteSelectAllCommand);
            MouseSelectionCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteMouseSelectionCommand);
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(e =>
                                                                             {
                                                                                 if (e.ActivePane == this) return;
                                                                                 IsActive = false;
                                                                                 CurrentRow = null;
                                                                             });
            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
        }

        public abstract void LoadDataAsync(LoadCommand cmd, object cmdParam);

        public abstract void DeleteAll();

        public abstract void CreateFolder(string name);

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            Parent.RaiseCanExecuteChanges();
        }

        public void Refresh()
        {
            ChangeDirectoryCommand.Execute(Stack.Peek().Path);
        }

        protected void SaveCache(FileSystemItem fileSystemItem, string path)
        {
            var fs = new FileStream(path, FileMode.Create);
            BinaryFormatter.Serialize(fs, fileSystemItem);
            fs.Flush();
            fs.Close();
        }

        protected FileSystemItem LoadCache(string path)
        {
            if (!File.Exists(path)) return null;
            var fs = new FileStream(path, FileMode.Open);
            var cachedItem = (FileSystemItem)BinaryFormatter.Deserialize(fs);
            fs.Close();
            return cachedItem;
        }
    }
}