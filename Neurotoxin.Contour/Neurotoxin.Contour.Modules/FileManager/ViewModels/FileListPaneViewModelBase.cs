using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.ContentProviders;
using Neurotoxin.Contour.Modules.FileManager.Events;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Microsoft.Practices.Composite;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public abstract class FileListPaneViewModelBase<T> : PaneViewModelBase, IFileListPaneViewModel where T : IFileManager
    {
        private bool _isInEditMode;
        private bool _isBusy;
        private string _sortMemberPath = "ComputedName";
        private ListSortDirection _listSortDirection = ListSortDirection.Ascending;
        private readonly Dictionary<FileSystemItemViewModel, Stack<FileSystemItemViewModel>> _stackCache = new Dictionary<FileSystemItemViewModel, Stack<FileSystemItemViewModel>>();
        private readonly TitleRecognizer _titleRecognizer;
        internal readonly T FileManager;

        #region Properties

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

        #endregion

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private bool CanExecuteChangeDirectoryCommand(object cmdParam)
        {
            if (_isInEditMode || _isBusy) return false;

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
            return true;
        }

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            var keyEvent = cmdParam as EventInformation<KeyEventArgs>;
            if (keyEvent != null) keyEvent.EventArgs.Handled = true;

            if (CurrentRow != null)
            {
                if (CurrentRow.Type == ItemType.File)
                {
                    if (CurrentRow.TitleType == TitleType.Profile)
                    {
                        Parent.OpenStfsPackage(CurrentRow);
                    }
                    return;
                }
                if (CurrentRow.IsUpDirectory)
                    _previouslyFocusedRow = Stack.Pop();
                else
                    Stack.Push(CurrentRow);
                NotifyPropertyChanged(CURRENTFOLDER);
            }
            _isBusy = true;
            WorkerThread.Run(ChangeDirectoryOuter, ChangeDirectoryCallback);
        }

        //TODO: Refactor
        private List<FileSystemItem> ChangeDirectoryOuter()
        {
            var content = ChangeDirectory();
            if (Stack.Count > 1)
            {
                var parentFolder = Stack.Peek();
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

        private List<FileSystemItem> ChangeDirectory(string selectedPath = null, bool recognize = true)
        {
            if (selectedPath == null) selectedPath = CurrentFolder.Path;
            var content = FileManager.GetList(selectedPath);

            foreach (var item in content)
            {
                if (recognize && (CurrentFolder.ContentType != ContentType.Undefined || _titleRecognizer.IsXboxFolder(item)))
                {
                    _titleRecognizer.RecognizeTitle(item);
                }
            }
            return content;
        }

        protected virtual void ChangeDirectoryCallback(List<FileSystemItem> result)
        {
            SortContent(result.Select(c => new FileSystemItemViewModel(c)));
            SetActiveCommand.Execute(null);
            NotifyPropertyChanged(SIZEINFO);
            _isBusy = false;
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
            SortContent();
            CurrentRow = selection;
            column.SortDirection = _listSortDirection;
        }

        private void SortContent()
        {
            SortContent(Items);
        }

        private void SortContent(IEnumerable<FileSystemItemViewModel> content)
        {
            if (content == null) return;

            var list = content.OrderByDescending(p => p.Type).ThenByProperty(_sortMemberPath, _listSortDirection).ToList();
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
                SelectIntervalOfItems(CurrentRow, item);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                item.IsSelected = true;
            }
            NotifyPropertyChanged(SIZEINFO);
        }

        private void SelectIntervalOfItems(FileSystemItemViewModel from, FileSystemItemViewModel to, bool value = true)
        {
            var start = Items.IndexOf(from);
            var end = Items.IndexOf(to);
            if (end < start)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }
            for (var i = start; i <= end; i++)
            {
                Items[i].IsSelected = value;
            }            
        }

        #endregion

        #region GoToFirstCommand

        public DelegateCommand<bool> GoToFirstCommand { get; private set; }

        private bool CanExecuteGoToFirstCommand(bool select)
        {
            return Items.Count > 0;
        }

        private void ExecuteGoToFirstCommand(bool select)
        {
            var first = Items.First();
            if (select) SelectIntervalOfItems(first, CurrentRow, !CurrentRow.IsSelected);
            CurrentRow = first;
        }

        #endregion

        #region GoToLastCommand

        public DelegateCommand<bool> GoToLastCommand { get; private set; }

        private bool CanExecuteGoToLastCommand(bool select)
        {
            return Items.Count > 0;
        }

        private void ExecuteGoToLastCommand(bool select)
        {
            var last = Items.Last();
            if (select) SelectIntervalOfItems(CurrentRow, last, !CurrentRow.IsSelected);
            CurrentRow = last;
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
            return CurrentRow != null && _titleRecognizer.HasCache(CurrentRow.Model);
        }

        private FileSystemItemViewModel RefreshTitle()
        {
            var result = CurrentRow;
            _titleRecognizer.RecognizeTitle(CurrentRow.Model, true);
            return result;
        }

        private static void RefreshTitleCallback(FileSystemItemViewModel item)
        {
            item.NotifyModelChanges();
        }

        #endregion

        #region CopyTitleIdToClipboardCommand

        public DelegateCommand CopyTitleIdToClipboardCommand { get; private set; }

        private bool CanExecuteCopyTitleIdToClipboardCommand()
        {
            return CurrentRow != null && CurrentRow.TitleType == TitleType.Game;
        }

        private void ExecuteCopyTitleIdToClipboardCommand()
        {
            Clipboard.SetData(DataFormats.Text, CurrentRow.Name);
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand SearchGoogleCommand { get; private set; }

        private bool CanExecuteSearchGoogleCommand()
        {
            return CurrentRow != null && CurrentRow.TitleType == TitleType.Game;
        }

        private void ExecuteSearchGoogleCommand()
        {
            System.Diagnostics.Process.Start(string.Format("http://www.google.com/#q={0}", CurrentRow.Name));
        }

        #endregion

        #region BeginRenameCommand

        public DelegateCommand<object> RenameCommand { get; private set; }

        private bool CanExecuteRenameCommand(object cmdParam)
        {
            return CurrentRow != null && _titleRecognizer.HasCache(CurrentRow.Model);
        }

        private void ExecuteRenameCommand(object cmdParam)
        {
            var grid = cmdParam as DataGrid;
            var row = grid != null ? grid.FindRowByValue(CurrentRow) : cmdParam as DataGridRow;
            if (row == null) return;
            row.FirstCell().IsEditing = true;
            _isInEditMode = true;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            CurrentRow.PropertyChanged += EndRename;
        }

        private void EndRename(object sender, PropertyChangedEventArgs e)
        {
            _isInEditMode = false;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            _titleRecognizer.SaveCache(CurrentRow.Model);
            CurrentRow.PropertyChanged -= EndRename;
        }

        #endregion

        protected FileListPaneViewModelBase(FileManagerViewModel parent, T fileManager) : base(parent)
        {
            FileManager = fileManager;
            _titleRecognizer = new TitleRecognizer(fileManager);

            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand, CanExecuteChangeDirectoryCommand);
            CalculateSizeCommand = new DelegateCommand<bool>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            ToggleSelectionCommand = new DelegateCommand<ToggleSelectionMode>(ExecuteToggleSelectionCommand);
            SelectAllCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteSelectAllCommand);
            MouseSelectionCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteMouseSelectionCommand);
            GoToFirstCommand = new DelegateCommand<bool>(ExecuteGoToFirstCommand, CanExecuteGoToFirstCommand);
            GoToLastCommand = new DelegateCommand<bool>(ExecuteGoToLastCommand, CanExecuteGoToLastCommand);
            RefreshTitleCommand = new DelegateCommand(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            CopyTitleIdToClipboardCommand = new DelegateCommand(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
            RenameCommand = new DelegateCommand<object>(ExecuteRenameCommand, CanExecuteRenameCommand);

            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
            Items = new ObservableCollection<FileSystemItemViewModel>();
        }

        public override void SetActive()
        {
            base.SetActive();
            if (CurrentRow != null)
            {
                CurrentRow = CurrentRow; //surely need to notify?!
                return;
            }
            if (_previouslyFocusedRow != null)
            {
                var previous = Items.FirstOrDefault(item => item.Path == _previouslyFocusedRow.Path);
                if (previous != null)
                {
                    CurrentRow = previous;
                    return;
                }
            }
            CurrentRow = Items.FirstOrDefault();
        }

        protected override void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            base.OnActivePaneChanged(e);
            _previouslyFocusedRow = CurrentRow;
            CurrentRow = null;
        }

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
            ChangeDirectoryCommand.Execute(null);
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

        public override void Refresh()
        {
            ChangeDirectoryCommand.Execute(null);
        }

        public byte[] ReadFileContent(string itemPath)
        {
            return FileManager.ReadFileContent(itemPath);
        }

        public FileSystemItemViewModel GetItemViewModel(string itemPath)
        {
            return new FileSystemItemViewModel(_titleRecognizer.RecognizeTitle(itemPath));
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
                {
                    var start = queue.Count;
                    PopulateQueue(queue, ChangeDirectory(item.Path, false).Select(c => new FileSystemItemViewModel(c)));
                    var end = queue.Count;
                    long size = 0;
                    for (var i = start; i < end; i++)
                    {
                        size += queue.ElementAt(i).Size ?? 0;
                    }
                    item.Size = size;
                }
            }
        }

    }
}