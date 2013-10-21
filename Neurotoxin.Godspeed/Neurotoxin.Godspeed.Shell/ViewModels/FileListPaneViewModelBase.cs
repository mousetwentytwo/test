using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Microsoft.Practices.Composite;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class FileListPaneViewModelBase<T> : PaneViewModelBase, IFileListPaneViewModel where T : IFileManager
    {
        private string _sortMemberPath = "ComputedName";
        private ListSortDirection _listSortDirection = ListSortDirection.Ascending;
        private Queue<FileSystemItem> _queue;
        private readonly Dictionary<FileSystemItemViewModel, string> _pathCache = new Dictionary<FileSystemItemViewModel, string>();
        private readonly TitleRecognizer _titleRecognizer;
        protected readonly T FileManager;

        #region Properties

        private const string ISINEDITMODE = "IsInEditMode";
        private bool _isInEditMode;
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set { _isInEditMode = value; NotifyPropertyChanged(ISINEDITMODE); }
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
                    if (CurrentFolder != null) _pathCache[_drive] = CurrentFolder.Path;
                    _drive = value;
                    ChangeDrive();
                }
                else
                {
                    NotificationMessage.Show("Drive change failed", string.Format("{0} is not accessible.", value.Title));
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
        private FileSystemItemViewModel _currentFolder;
        public FileSystemItemViewModel CurrentFolder
        {
            get { return _currentFolder; }
            set { _currentFolder = value; NotifyPropertyChanged(CURRENTFOLDER); }
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
            set 
            {
                if (_currentRow == value) return;
                _currentRow = value; 
                NotifyPropertyChanged(CURRENTROW);
            }
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
            if (IsInEditMode || IsBusy) return false;

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
                    if (CurrentRow.TitleType == TitleType.Profile) OpenStfsPackage();
                    return;
                }

                if (CurrentRow.IsUpDirectory)
                {
                    var r = new Regex(@"^(.*[\\/]).*?[\\/]$");
                    var parentPath = r.Replace(CurrentRow.Path, "$1");
                    if (Drive.Path == parentPath)
                    {
                        CurrentFolder = Drive;
                    }
                    else
                    {
                        var folder = FileManager.GetFolderInfo(parentPath);
                        if (folder == null)
                        {
                            NotificationMessage.Show("Navigation error", string.Format("Can't find path: {0}", parentPath));
                            CurrentFolder = Drive;
                        } 
                        else
                        {
                            CurrentFolder = new FileSystemItemViewModel(folder);    
                        }
                    }
                } 
                else
                {
                    CurrentFolder = CurrentRow;
                }
            }

            ProgressMessage = "Changing directory...";
            IsBusy = true;           
            WorkerThread.Run(() => ChangeDirectory(CurrentFolder.Path), ChangeDirectoryCallback, AsyncErrorCallback);
        }

        private List<FileSystemItem> ChangeDirectory(string selectedPath)
        {
            var list = FileManager.GetList(selectedPath);
            list.ForEach(item => _titleRecognizer.RecognizeType(item));
            return list;
        }

        private void ChangeDirectoryCallback(List<FileSystemItem> result)
        {
            IsBusy = false;

            _queue = new Queue<FileSystemItem>();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var item in result.Where(item => !_titleRecognizer.MergeWithCachedEntry(item)))
            {
                if (CurrentFolder.ContentType == ContentType.Undefined && !_titleRecognizer.IsXboxFolder(item)) continue;
                _queue.Enqueue(item);
            }
            sw.Stop();
            Debug.WriteLine(sw.Elapsed);

            if (CurrentFolder.Type != ItemType.Drive)
            {
                result.Insert(0, new FileSystemItem
                {
                    Title = "[..]",
                    Type = CurrentFolder.Type,
                    Date = CurrentFolder.Date,
                    Path = CurrentFolder.Path,
                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/up.png")
                });
            }

            SortContent(result.Select(c => new FileSystemItemViewModel(c)));
            NotifyPropertyChanged(SIZEINFO);

            if (_queue.Count > 0)
            {
                IsBusy = true;
                RecognitionStart();
            }
            else
            {
                RecognitionFinish();
            }
        }

        private void OpenStfsPackage()
        {
            ProgressMessage = string.Format("Opening profile {0}...", CurrentRow.ComputedName);
            IsBusy = true;
            WorkerThread.Run(() => ReadFileContent(CurrentRow), OpenStfsPackageCallback, AsyncErrorCallback);
        }

        private void OpenStfsPackageCallback(byte[] content)
        {
            var stfs = container.Resolve<StfsPackageContentViewModel>();
            stfs.LoadDataAsync(LoadCommand.Load, content, OpenStfsPackageSuccess, OpenStfsPackageError);
        }

        private void OpenStfsPackageSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            eventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void OpenStfsPackageError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            NotificationMessage.Show("Open failed", string.Format("Can't open {0}: {1}", CurrentRow.ComputedName, exception.Message));
        }

        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<bool> CalculateSizeCommand { get; private set; }
        private Queue<FileSystemItemViewModel> _calculationQueue;
        private bool _calculationIsRunning = false;

        private void ExecuteCalculateSizeCommand(bool calculateAll)
        {
            if (calculateAll)
            {
                if (_calculationQueue == null) _calculationQueue = new Queue<FileSystemItemViewModel>();
                foreach (var item in SelectedItems.Where(item => item.Type == ItemType.Directory && !_calculationQueue.Contains(item)))
                {
                    _calculationQueue.Enqueue(item);
                }
            } 
            else if (CurrentRow.Type == ItemType.Directory)
            {
                if (_calculationQueue == null) _calculationQueue = new Queue<FileSystemItemViewModel>();
                if (!_calculationQueue.Contains(CurrentRow)) _calculationQueue.Enqueue(CurrentRow);
            }

            if (_calculationQueue == null || _calculationQueue.Count <= 0 || _calculationIsRunning) return;

            _calculationIsRunning = true;
            WorkerThread.Run(CalculateSize, CalculateSizeCallback, AsyncErrorCallback);
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
                WorkerThread.Run(CalculateSize, CalculateSizeCallback, AsyncErrorCallback);
            } 
            else
            {
                _calculationQueue = null;
                _calculationIsRunning = false;
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

            var currentRow = CurrentRow;
            Items.Clear();
            Items.AddRange(list);
            CurrentRow = currentRow;
            SetActive();
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

        private bool CanExecuteRefreshTitleCommand()
        {
            return CurrentRow != null;
        }

        private void ExecuteRefreshTitleCommand()
        {
            var selection = SelectedItems.Any() ? SelectedItems.Select(i => i.Model) : new[] {CurrentRow.Model};
            selection.ForEach(_titleRecognizer.ThrowCache);
            _queue = new Queue<FileSystemItem>(selection);
            IsBusy = true;
            RecognitionStart();
        }

        #endregion

        #region RecognizeFromProfileCommand

        public DelegateCommand RecognizeFromProfileCommand { get; private set; }

        private bool CanExecuteRecognizeFromProfileCommand()
        {
            return CurrentRow != null && CurrentRow.IsProfile;
        }

        private void ExecuteRecognizeFromProfileCommand()
        {
            IsBusy = true;
            ProgressMessage = "Scanning profile...";
            WorkerThread.Run(RecognizeFromProfile, RecognizeFromProfileCallback, AsyncErrorCallback);
        }

        private int RecognizeFromProfile()
        {
            var tmp = _titleRecognizer.GetTempFilePath(CurrentRow.Model);
            if (tmp == null) return -1;

            var i = 0;
            var stfs = ModelFactory.GetModel<StfsPackage>(tmp);
            stfs.ExtractContent();
            stfs.ProfileInfo.TitlesPlayed.ForEach(g =>
                                                      {
                                                          var game = stfs.Games.Values.FirstOrDefault(gg => gg.TitleId == g.TitleCode);
                                                          if (game == null) return;
                                                          var cacheEntry = _titleRecognizer.GetCacheEntry(g.TitleCode);
                                                          if (cacheEntry != null && cacheEntry.Expiration == null) return;
                                                          var item = new FileSystemItem
                                                                         {
                                                                             Name = g.TitleCode,
                                                                             Title = g.TitleName,
                                                                             Type = ItemType.Directory,
                                                                             TitleType = TitleType.Game,
                                                                             Thumbnail = game.Thumbnail
                                                                         };
                                                          _titleRecognizer.UpdateCache(item);
                                                          i++;
                                                      });
            return i;
        }

        private void RecognizeFromProfileCallback(int count)
        {
            IsBusy = false;
            var message = count < 1 ? "No new titles found." : string.Format("{0} new title{1} found.", count, count > 1 ? "s" : string.Empty);
            NotificationMessage.Show("Title Recognition", message);
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
            return CurrentRow != null && CurrentRow.IsCached;
        }

        private void ExecuteRenameCommand(object cmdParam)
        {
            var grid = cmdParam as DataGrid;
            var row = grid != null ? grid.FindRowByValue(CurrentRow) : cmdParam as DataGridRow;
            if (row == null) return;
            var cell = row.FirstCell();
            grid.CellEditEnding += GridOnCellEditEnding;
            cell.IsEditing = true;
            IsInEditMode = true;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            CurrentRow.PropertyChanged += EndRename;
        }

        private void GridOnCellEditEnding(object sender, DataGridCellEditEndingEventArgs dataGridCellEditEndingEventArgs)
        {
            IsInEditMode = false;
        }

        private void EndRename(object sender, PropertyChangedEventArgs e)
        {
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            _titleRecognizer.UpdateCache(CurrentRow.Model);
            CurrentRow.PropertyChanged -= EndRename;
        }

        #endregion

        protected FileListPaneViewModelBase(FileManagerViewModel parent) : base(parent)
        {
            FileManager = container.Resolve<T>();
            _titleRecognizer = new TitleRecognizer(FileManager, container.Resolve<CacheManager>());

            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand, CanExecuteChangeDirectoryCommand);
            CalculateSizeCommand = new DelegateCommand<bool>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            ToggleSelectionCommand = new DelegateCommand<ToggleSelectionMode>(ExecuteToggleSelectionCommand);
            SelectAllCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteSelectAllCommand);
            MouseSelectionCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteMouseSelectionCommand);
            GoToFirstCommand = new DelegateCommand<bool>(ExecuteGoToFirstCommand, CanExecuteGoToFirstCommand);
            GoToLastCommand = new DelegateCommand<bool>(ExecuteGoToLastCommand, CanExecuteGoToLastCommand);
            RefreshTitleCommand = new DelegateCommand(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            RecognizeFromProfileCommand = new DelegateCommand(ExecuteRecognizeFromProfileCommand, CanExecuteRecognizeFromProfileCommand);
            CopyTitleIdToClipboardCommand = new DelegateCommand(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
            RenameCommand = new DelegateCommand<object>(ExecuteRenameCommand, CanExecuteRenameCommand);

            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
            Items = new ObservableCollection<FileSystemItemViewModel>();
        }

        public abstract string GetTargetPath(string path);
        protected abstract void SaveToFileStream(string path, FileStream fs, long remoteStartPosition);
        protected abstract void CreateFile(string targetPath, string sourcePath);
        protected abstract void OverwriteFile(string targetPath, string sourcePath);
        protected abstract void ResumeFile(string targetPath, string sourcePath);

        public override void SetActive()
        {
            base.SetActive();
            if (CurrentRow != null)
            {
                CurrentRow = Items.FirstOrDefault(item => item.Path == CurrentRow.Path);
                return;
            }
            CurrentRow = Items.FirstOrDefault();
        }

        public bool Delete(FileSystemItem item)
        {
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

        public bool CreateFolder(string path)
        {
            if (FileManager.FolderExists(path)) return false;
            FileManager.CreateFolder(path);
            return true;
        }

        private bool IsDriveAccessible(FileSystemItemViewModel drive)
        {
            try
            {
                return FileManager.DriveIsReady(drive.Path);
            }
            catch (TransferException ex)
            {
                Parent.ShowCorrespondingErrorDialog(ex);
                return false;
            }
        }

        protected virtual void ChangeDrive()
        {
            CurrentRow = null;
            CurrentFolder = _pathCache.ContainsKey(Drive) ? new FileSystemItemViewModel(FileManager.GetFolderInfo(_pathCache[Drive])) : Drive;
            ChangeDirectoryCommand.Execute(null);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            CalculateSizeCommand.RaiseCanExecuteChanged();
            RefreshTitleCommand.RaiseCanExecuteChanged();
            RecognizeFromProfileCommand.RaiseCanExecuteChanged();
            CopyTitleIdToClipboardCommand.RaiseCanExecuteChanged();
            SearchGoogleCommand.RaiseCanExecuteChanged();
            RenameCommand.RaiseCanExecuteChanged();
            Parent.RaiseCanExecuteChanges();
        }

        public override void Refresh(Action callback)
        {
            ProgressMessage = "Refreshing directory...";
            IsBusy = true;
            WorkerThread.Run(
                () => ChangeDirectory(CurrentFolder.Path), 
                result =>
                    {
                        ChangeDirectoryCallback(result);
                        if (callback != null) callback.Invoke();
                    }, 
                AsyncErrorCallback);
        }

        private byte[] ReadFileContent(FileSystemItemViewModel item)
        {
            item.TempFilePath = _titleRecognizer.GetTempFilePath(item.Model);
            return item.TempFilePath != null
                ? File.ReadAllBytes(item.TempFilePath) 
                : FileManager.ReadFileContent(item.Path);
        }

        public void GetItemViewModel(string itemPath)
        {
            var listedItem = Items.FirstOrDefault(item => item.Path == itemPath);
            if (listedItem != null)
            {
                PublishItemViewModel(listedItem);
                return;
            }

            WorkerThread.Run(() => FileManager.GetFileInfo(itemPath), (item) =>
                {
                    var vm = new FileSystemItemViewModel(item);
                    RecognitionInner(item, i => PublishItemViewModel(vm));
                });
        }

        private void RecognitionStart()
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Dequeue();
                ProgressMessage = string.Format("Recognizing item {0}... ({1} left)", item.Name, _queue.Count);
                RecognitionInner(item, RecognitionSuccess);
            }
            else
            {
                RecognitionFinish();
            }
        }

        private void RecognitionInner(FileSystemItem item, Action<FileSystemItem> callback)
        {
            WorkerThread.Run(() =>
            {
                _titleRecognizer.RecognizeTitle(item);
                return item;
            }, callback);
        }

        private void RecognitionSuccess(FileSystemItem item)
        {
            Items.Single(i => i.Model == item).NotifyModelChanges();
            RecognitionStart();
        }

        private void RecognitionFinish()
        {
            _queue = null;
            SortContent();
            IsBusy = false;
        }

        private void PublishItemViewModel(ViewModelBase vm)
        {
            eventAggregator.GetEvent<ViewModelGeneratedEvent>().Publish(new ViewModelGeneratedEventArgs(vm));
        }

        public Queue<FileSystemItem> PopulateQueue()
        {
            return PopulateQueue(false);
        }

        public Queue<FileSystemItem> PopulateQueue(bool bottomToTop)
        {
            var res = PopulateQueue(SelectedItems.Any() ? SelectedItems.Select(vm => vm.Model) : new[] { CurrentRow.Model }, bottomToTop);
            var queue = new Queue<FileSystemItem>();
            res.ForEach(queue.Enqueue);
            return queue;
        }

        private List<FileSystemItem> PopulateQueue(IEnumerable<FileSystemItem> items, bool bottomToTop)
        {
            var result = new List<FileSystemItem>();
            foreach (var item in items)
            {
                if (!bottomToTop) result.Add(item);
                if (item.Type == ItemType.Directory)
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var sub = PopulateQueue(ChangeDirectory(item.Path), bottomToTop);
                    sw.Stop();
                    var x = sw.Elapsed;
                    sw.Reset();
                    sw.Start();
                    item.Size = sub.Sum(i => i.Size ?? 0);
                    result.AddRange(sub);
                    sw.Stop();
                    Debug.WriteLine("{0} {1} p:{2} s:{3}", item.Name, result.Count, x, sw.Elapsed);
                }
                if (bottomToTop) result.Add(item);
            }
            return result;
        }

        private void AsyncErrorCallback(Exception ex)
        {
            _calculationIsRunning = false;
            IsBusy = false;
            Parent.ShowCorrespondingErrorDialog(ex);
        }

        public bool Export(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            FileMode mode;
            long remoteStartPosition = 0;
            switch (action)
            {
                case CopyAction.CreateNew:
                    if (FileManager.FileExists(savePath))
                        throw new TransferException(TransferErrorType.WriteAccessError, item.Path, savePath, "Target already exists");
                    mode = FileMode.CreateNew;
                    break;
                case CopyAction.Overwrite:
                    mode = FileMode.Create;
                    break;
                case CopyAction.OverwriteOlder:
                    var fileDate = File.GetLastWriteTime(savePath);
                    if (fileDate > item.Date) return false;
                    mode = FileMode.Create;
                    break;
                case CopyAction.Resume:
                    mode = FileMode.Append;
                    var fi = new FileInfo(savePath);
                    remoteStartPosition = fi.Length;
                    break;
                default:
                    throw new ArgumentException("Invalid Copy action: " + action);
            }
            var fs = new FileStream(savePath, mode);
            SaveToFileStream(item.Path, fs, remoteStartPosition);
            fs.Flush();
            fs.Close();
            return true;
        }

        public bool Import(FileSystemItem item, string savePath, CopyAction action)
        {
            if (item.Type != ItemType.File) throw new NotSupportedException();
            var itemPath = item.Path;
            switch (action)
            {
                case CopyAction.CreateNew:
                    if (FileManager.FileExists(savePath))
                        throw new TransferException(TransferErrorType.WriteAccessError, itemPath, savePath, "Target already exists");
                    CreateFile(savePath, itemPath);
                    break;
                case CopyAction.Overwrite:
                    OverwriteFile(savePath, itemPath);
                    break;
                case CopyAction.OverwriteOlder:
                    var fileDate = FileManager.GetFileModificationTime(savePath);
                    if (fileDate > item.Date) return false;
                    OverwriteFile(savePath, itemPath);
                    break;
                case CopyAction.Resume:
                    ResumeFile(savePath, itemPath);
                    break;
                default:
                    throw new ArgumentException("Invalid Copy action: " + action);
            }
            return true;
        }

    }
}