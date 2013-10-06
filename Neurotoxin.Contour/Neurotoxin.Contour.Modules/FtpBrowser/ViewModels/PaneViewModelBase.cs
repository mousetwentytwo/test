﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private FileSystemItemViewModel _previouslyFocusedRow;

        private const string CURRENTROW = "CurrentRow";
        private FileSystemItemViewModel _currentRow;
        public FileSystemItemViewModel CurrentRow
        {
            get { return _currentRow; }
            set { _currentRow = value; NotifyPropertyChanged(CURRENTROW); RaiseCanExecuteChanges(); }
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
            SetActive();
        }

        public void SetActive()
        {
            IsActive = true;
            CurrentRow = CurrentRow ?? _previouslyFocusedRow ?? Items.FirstOrDefault();
        }

        #endregion

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

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

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null) keyEvent.EventArgs.Handled = true;

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

        private List<FileSystemItem> ChangeDirectory(string selectedPath = null)
        {
            var recognize = false;
            if (selectedPath == null)
            {
                recognize = true;
                selectedPath = CurrentFolder.Path;
            }

            var content = FileManager.GetList(selectedPath);

            foreach (var item in content)
            {
                switch (item.Type)
                {
                    case ItemType.Directory:
                        item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                        break;
                    case ItemType.File:
                        item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/file.png");
                        break;
                    default:
                        throw new NotSupportedException();
                }
                if (recognize && (CurrentFolder.Subtype != ItemSubtype.Undefined || TitleManager.IsXboxFolder(item)))
                {
                    TitleManager.RecognizeTitle(item, CurrentFolder.Model);
                }
            }
            return content;
        }

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

        private bool CanExecuteCalculateSizeCommand(bool calculateAll)
        {
            return true;
        }

        private long CalculateSize()
        {
            return CalculateSize(_calculationQueue.Peek().Path);
        }

        private long CalculateSize(string path)
        {
            var list = FileManager.GetList(path);
            return list.Where(item => item.Type == ItemType.File).Sum(fi => fi.Size.HasValue ? fi.Size.Value : 0)
                 + list.Where(item => item.Type == ItemType.Directory).Sum(di => CalculateSize(string.Format("{0}{1}/", path, di.Name)));
        }

        private void CalculateSizeCallback(long size)
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
                                 ? collection.ThenBy(p => p.Name)
                                 : collection.ThenByDescending(p => p.Name);

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

        #region RefreshTitleCommand

        public DelegateCommand RefreshTitleCommand { get; private set; }

        private void ExecuteRefreshTitleCommand()
        {
            WorkerThread.Run(RefreshTitle, RefreshTitleCallback);
        }

        private bool CanExecuteRefreshTitleCommand()
        {
            return CurrentRow != null && TitleManager.HasCache(CurrentRow.Model);
        }

        private FileSystemItemViewModel RefreshTitle()
        {
            var result = CurrentRow;
            var model = CurrentRow.Model;
            TitleManager.RecognizeTitle(model, CurrentFolder.Model);
            return result;
        }

        private static void RefreshTitleCallback(FileSystemItemViewModel item)
        {
            item.NotifyModelChanges();
        }

        #endregion

        #region CopyTitleIdToClipboardCommand

        public DelegateCommand CopyTitleIdToClipboardCommand { get; private set; }

        private void ExecuteCopyTitleIdToClipboardCommand()
        {
            Clipboard.SetData(DataFormats.Text, CurrentRow.Name);
        }

        private bool CanExecuteCopyTitleIdToClipboardCommand()
        {
            return CurrentRow != null && TitleManager.IsXboxFolder(CurrentRow.Model);
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand SearchGoogleCommand { get; private set; }

        private void ExecuteSearchGoogleCommand()
        {
            System.Diagnostics.Process.Start(string.Format("http://www.google.com/#q={0}", CurrentRow.Name));
        }

        private bool CanExecuteSearchGoogleCommand()
        {
            return CurrentRow != null && TitleManager.IsXboxFolder(CurrentRow.Model);
        }

        #endregion

        #region BeginRenameCommand

        public DelegateCommand<object> RenameCommand { get; private set; }

        private bool CanExecuteRenameCommand(object cmdParam)
        {
            return CurrentRow != null && TitleManager.HasCache(CurrentRow.Model);
        }

        private void ExecuteRenameCommand(object cmdParam)
        {
            var grid = cmdParam as DataGrid;
            var row = grid != null ? grid.FindRowByValue(CurrentRow) : cmdParam as DataGridRow;
            if (row == null) return;
            row.FirstCell().IsEditing = true;
            IsInEditMode = true;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            CurrentRow.PropertyChanged += EndRename;
        }

        private void EndRename(object sender, PropertyChangedEventArgs e)
        {
            IsInEditMode = false;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            TitleManager.SaveCache(CurrentRow.Model);
            CurrentRow.PropertyChanged -= EndRename;
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
            RefreshTitleCommand = new DelegateCommand(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            CopyTitleIdToClipboardCommand = new DelegateCommand(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
            RenameCommand = new DelegateCommand<object>(ExecuteRenameCommand, CanExecuteRenameCommand);

            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(e =>
                                                                             {
                                                                                 if (e.ActivePane == this) return;
                                                                                 IsActive = false;
                                                                                 _previouslyFocusedRow = CurrentRow;
                                                                                 CurrentRow = null;
                                                                             });
            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
        }

        public abstract void LoadDataAsync(LoadCommand cmd, object cmdParam);

        public bool Delete(FileSystemItemViewModel item)
        {
            //TODO: handle errors

            if (item.Type == ItemType.Directory)
            {
                FileManager.DeleteFolder(item.Path);
            }
            else
            {
                FileManager.DeleteFile(item.Path);
            }
            return true;
        }

        public bool CreateFolder(string name)
        {
            var path = string.Format("{0}{1}", CurrentFolder.Path, name);
            FileManager.CreateFolder(path);
            return true;
        }

        private bool IsDriveAccessible(FileSystemItemViewModel drive)
        {
            var result = FileManager.DriveIsReady(drive.Path);
            if (!result)
            {
                MessageBox.Show(string.Format("{0} is not accessible.", drive.Title));
            }
            return result;
        }

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
            CopyTitleIdToClipboardCommand.RaiseCanExecuteChanged();
            SearchGoogleCommand.RaiseCanExecuteChanged();
            RenameCommand.RaiseCanExecuteChanged();
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