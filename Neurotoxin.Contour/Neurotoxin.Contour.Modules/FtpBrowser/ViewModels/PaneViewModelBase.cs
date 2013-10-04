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
using System.Windows.Input;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using Expression = System.Linq.Expressions.Expression;
using Microsoft.Practices.Composite;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public abstract class PaneViewModelBase<T> : ViewModelBase, IPaneViewModel where T : IFileManager
    {
        protected bool IsInEditMode;

        #region Properties

        internal readonly T FileManager;
        internal readonly TitleManager<T> TitleManager;

        protected ModuleViewModelBase Parent { get; private set; }

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

        private const string DRIVES = "Drives";
        private ObservableCollection<FileSystemItemViewModel> _drives;
        public ObservableCollection<FileSystemItemViewModel> Drives
        {
            get { return _drives; }
            set { _drives = value; NotifyPropertyChanged(DRIVES); }
        }

        private const string DRIVE = "Drive";
        private FileSystemItemViewModel _drive;
        public FileSystemItemViewModel Drive
        {
            get { return _drive; }
            set
            {
                if (IsDriveAccessible(value))
                {
                    _drive = value;
                    ChangeDrive();
                }
                NotifyPropertyChanged(DRIVE);
            }
        }

        private const string DRIVELABEL = "DriveLabel";
        private string _driveLabel;
        public string DriveLabel
        {
            get { return _driveLabel; }
            set { _driveLabel = value; NotifyPropertyChanged(DRIVELABEL); }
        }

        private const string FREESPACE = "FreeSpace";
        private string _freeSpace;
        public string FreeSpace
        {
            get { return _freeSpace; }
            set { _freeSpace = value; NotifyPropertyChanged(FREESPACE); }
        }

        private const string STACK = "Stack";
        private Stack<FileSystemItemViewModel> _stack;
        public Stack<FileSystemItemViewModel> Stack
        {
            get { return _stack; }
            set { _stack = value; NotifyPropertyChanged(STACK); }
        }

        private const string CURRENTFOLDER = "CurrentFolder";
        public FileSystemItemViewModel CurrentFolder
        {
            get { return _stack != null && _stack.Count > 0 ? _stack.Peek() : null; }
        }

        private const string ITEMS = "Items";
        private ObservableCollection<FileSystemItemViewModel> _items;
        public ObservableCollection<FileSystemItemViewModel> Items
        {
            get { return _items; }
            private set { _items = value; NotifyPropertyChanged(ITEMS); }
        }

        public IEnumerable<FileSystemItemViewModel> SelectedItems
        {
            get { return Items.Where(item => item.IsSelected); }
        }

        private const string CURRENTROW = "CurrentRow";
        private FileSystemItemViewModel _currentRow;
        public FileSystemItemViewModel CurrentRow
        {
            get { return _currentRow; }
            set { _currentRow = value; NotifyPropertyChanged(CURRENTROW); }
        }

        private const string SIZEINFO = "SizeInfo";
        public string SizeInfo
        {
            get
            {
                if (Items == null) return null;

                var selectedSize = Items.Where(item => item.Size != null && item.IsSelected).Sum(item => item.Size.Value);
                var totalSize = Items.Where(item => item.Size != null).Sum(item => item.Size.Value);
                var selectedFileCount = Items.Count(item => item.Type == ItemType.File && item.IsSelected);
                var totalFileCount = Items.Count(item => item.Type == ItemType.File);
                var selectedDirCount = Items.Count(item => item.Type == ItemType.Directory && item.IsSelected);
                var totalDirCount = Items.Count(item => item.Type == ItemType.Directory && !item.IsUpDirectory);
                const string s = "s";

                return string.Format("{0:#,0} / {1:#,0} bytes in {2} / {3} file{4}, {5} / {6} dir{7}", selectedSize,
                                     totalSize, selectedFileCount, totalFileCount, totalFileCount > 1 ? s : string.Empty,
                                     selectedDirCount, totalDirCount, totalDirCount > 1 ? s : string.Empty);
            }
        }        

        private string _sortMemberPath;
        private ListSortDirection _listSortDirection;
        private readonly Dictionary<FileSystemItemViewModel, Stack<FileSystemItemViewModel>> _stackCache = new Dictionary<FileSystemItemViewModel, Stack<FileSystemItemViewModel>>();

        #endregion

        #region SetActiveCommand

        public DelegateCommand<EventInformation<MouseEventArgs>> SetActiveCommand { get; private set; }

        private void ExecuteSetActiveCommand(EventInformation<MouseEventArgs> eventInformation)
        {
            IsActive = true;
            if (CurrentRow == null) CurrentRow = Items.FirstOrDefault();
        }

        #endregion

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            if (CurrentRow != null)
            {
                if (CurrentRow.IsUpDirectory)
                    Stack.Pop();
                else
                    Stack.Push(CurrentRow);
            }
            NotifyPropertyChanged(CURRENTFOLDER);
            WorkerThread.Run(ChangeDirectoryOuter, ChangeDirectoryCallback);
        }

        private bool CanExecuteChangeDirectoryCommand(object cmdParam)
        {
            if (IsInEditMode) return false;

            var item = cmdParam as FileSystemItemViewModel;
            if (item != null)
            {
                return item.Type != ItemType.File;
            }

            var mouseEvent = cmdParam as EventInformation<MouseEventArgs>;
            if (mouseEvent != null)
            {
                var e = mouseEvent.EventArgs;
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                if (!(dataContext is FileSystemItemViewModel)) return false;
            }

            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null)
            {
                var e = keyEvent.EventArgs;
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                if (!(dataContext is FileSystemItemViewModel)) return false;
                return e.Key == Key.Enter;
            }

            return CurrentRow == null || CurrentRow.Type != ItemType.File;
        }

        //TODO: Refactor
        private List<FileSystemItem> ChangeDirectoryOuter()
        {
            var content = ChangeDirectory();
            if (Stack.Count > 1)
            {
                var parentFolder = Stack.ElementAt(1);
                content.Insert(0, new FileSystemItem
                {
                    Title = "[..]",
                    Type = parentFolder.Type,
                    Date = parentFolder.Date,
                    Path = parentFolder.Path,
                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/up.png")
                });
            }
            return content;
        }

        internal abstract List<FileSystemItem> ChangeDirectory(string selectedPath = null);

        protected virtual void ChangeDirectoryCallback(List<FileSystemItem> result)
        {
            SortContent(result.Select(c => new FileSystemItemViewModel(c)));
            SetActiveCommand.Execute(null);

            NotifyPropertyChanged(SIZEINFO);
        }


        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<bool> CalculateSizeCommand { get; private set; }
        private Queue<FileSystemItemViewModel> _calculationQueue;

        private void ExecuteCalculateSizeCommand(bool calculateAll)
        {
            if (calculateAll)
            {
                _calculationQueue = new Queue<FileSystemItemViewModel>(SelectedItems.Where(item => item.Type == ItemType.Directory));
            } 
            else if (CurrentRow.Type == ItemType.Directory)
            {
                _calculationQueue = new Queue<FileSystemItemViewModel>();
                _calculationQueue.Enqueue(CurrentRow);
            }
            if (_calculationQueue != null && _calculationQueue.Count > 0)
            {
                WorkerThread.Run(CalculateSize, CalculateSizeCallback);
            }
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
            NotifyPropertyChanged(SIZEINFO);
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
            NotifyPropertyChanged(SIZEINFO);
        }

        #endregion

        #region SelectAllCommand

        public DelegateCommand<EventInformation<EventArgs>> SelectAllCommand { get; private set; }

        private void ExecuteSelectAllCommand(EventInformation<EventArgs> cmdParam)
        {
            Items.Where(row => !row.IsUpDirectory).ForEach(row => row.IsSelected = true);
            NotifyPropertyChanged(SIZEINFO);
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
            NotifyPropertyChanged(SIZEINFO);
        }

        #endregion

        #region EndEditCommand

        public DelegateCommand<EventInformation<EventArgs>> EndEditCommand { get; private set; }

        private void ExecuteEndEditCommand(EventInformation<EventArgs> eventInformation)
        {
            IsInEditMode = false;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region RefreshTitleCommand

        public DelegateCommand<FileSystemItemViewModel> RefreshTitleCommand { get; private set; }

        private void ExecuteRefreshTitleCommand(FileSystemItemViewModel cmdParam)
        {
            WorkerThread.Run(RefreshTitle, RefreshTitleCallback);
        }

        private bool CanExecuteRefreshTitleCommand(FileSystemItemViewModel cmdParam)
        {
            return true;
        }

        private FileSystemItemViewModel RefreshTitle()
        {
            var result = CurrentRow;
            var model = CurrentRow.Model;
            TitleManager.RecognizeTitle(model, CurrentFolder);
            return result;
        }

        private void RefreshTitleCallback(FileSystemItemViewModel item)
        {
            item.NotifyModelChanges();
        }

        #endregion

        protected PaneViewModelBase(ModuleViewModelBase parent, T fileManager)
        {
            Parent = parent;
            FileManager = fileManager;
            TitleManager = new TitleManager<T>(fileManager);
            SetActiveCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteSetActiveCommand);
            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand, CanExecuteChangeDirectoryCommand);
            CalculateSizeCommand = new DelegateCommand<bool>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            ToggleSelectionCommand = new DelegateCommand<ToggleSelectionMode>(ExecuteToggleSelectionCommand);
            SelectAllCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteSelectAllCommand);
            MouseSelectionCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteMouseSelectionCommand);
            EndEditCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteEndEditCommand);
            RefreshTitleCommand = new DelegateCommand<FileSystemItemViewModel>(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(e =>
                                                                             {
                                                                                 if (e.ActivePane == this) return;
                                                                                 IsActive = false;
                                                                                 CurrentRow = null;
                                                                             });
            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
        }

        public abstract void LoadDataAsync(LoadCommand cmd, object cmdParam);

        public abstract bool Delete(FileSystemItemViewModel item);

        public abstract bool CreateFolder(string name);

        protected abstract bool IsDriveAccessible(FileSystemItemViewModel drive);

        protected virtual void ChangeDrive()
        {
            if (_stackCache.ContainsKey(Drive))
            {
                Stack = _stackCache[Drive];
            }
            else
            {
                Stack = new Stack<FileSystemItemViewModel>();
                Stack.Push(Drive);
                _stackCache.Add(Drive, Stack);
            }
            CurrentRow = null;
            ChangeDirectoryCommand.Execute(Stack.Peek().Path);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            RefreshTitleCommand.RaiseCanExecuteChanged();
            Parent.RaiseCanExecuteChanges();
        }

        public void Refresh()
        {
            ChangeDirectoryCommand.Execute(Stack.Peek().Path);
        }

        public Queue<FileSystemItemViewModel> PopulateQueue()
        {
            var queue = new Queue<FileSystemItemViewModel>();
            PopulateQueue(queue, SelectedItems.Any() ? SelectedItems : new[] { CurrentRow });
            return queue;
        }

        private void PopulateQueue(Queue<FileSystemItemViewModel> queue, IEnumerable<FileSystemItemViewModel> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
                if (item.Type == ItemType.Directory)
                    PopulateQueue(queue, ChangeDirectory(item.Path).Select(c => new FileSystemItemViewModel(c)));
            }
        }

    }
}