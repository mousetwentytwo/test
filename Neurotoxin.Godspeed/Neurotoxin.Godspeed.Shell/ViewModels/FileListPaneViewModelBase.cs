using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Unity;
using Microsoft.Win32;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Formatters;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Core.Extensions;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class FileListPaneViewModelBase<T> : PaneViewModelBase, IFileListPaneViewModel where T : IFileManager
    {
        private readonly object _queueLock = new object();
        private Queue<FileSystemItem> _queue;
        protected readonly ITitleRecognizer TitleRecognizer;
        protected readonly T FileManager;
        protected readonly IUserSettings UserSettings;
        protected readonly Dictionary<FileSystemItemViewModel, string> PathCache = new Dictionary<FileSystemItemViewModel, string>();

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
                    if (CurrentFolder != null) PathCache[_drive] = CurrentFolder.Path;
                    _drive = value;
                }
                else
                {
                    var name = string.IsNullOrEmpty(value.Title)
                                   ? value.Name
                                   : string.Format("{0} ({1})", value.Title, value.Name);
                    WindowManager.ShowMessage(Resx.DriveChangeFailed, string.Format(Resx.DriveIsNotAccessible, name));
                    if (_drive == null) _drive = Drives.FirstOrDefault();
                }
                ChangeDrive();
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

                return string.Format(new PluralFormatProvider(), Resx.SizeInfo, selectedSize, totalSize, selectedFileCount, totalFileCount, selectedDirCount, totalDirCount);
            }
        }

        private const string ISRESUMESUPPORTED = "IsResumeSupported";
        private bool _isResumeSupported;
        public bool IsResumeSupported
        {
            get { return _isResumeSupported; }
            protected set { _isResumeSupported = value; NotifyPropertyChanged(ISRESUMESUPPORTED); }
        }

        public bool HasValidSelection
        {
            get { return SelectedItems.Any() || CurrentRow != null && !CurrentRow.IsUpDirectory; }
        }

        public abstract bool IsReadOnly { get; }

        public abstract bool IsVerificationSupported { get; }
        public abstract bool IsVerificationEnabled { get; }

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
                    if (CurrentRow.IsCompressedFile) OpenCompressedFileCommand.Execute();
                    //TODO: Do STFS check instead
                    if (CurrentRow.TitleType == TitleType.Profile) OpenStfsPackageCommand.Execute(OpenStfsPackageMode.Browsing);
                    return;
                }
                var destination = CurrentRow.IsUpDirectory ? UpDirectory() : CurrentRow;
                if (destination == null) return;
                if (destination.Name.Contains("?"))
                {
                    WindowManager.ShowMessage(Resx.IOError, Resx.SpecialCharactersNotSupported);
                    return;
                }
                CurrentFolder = destination;
            }
            ChangeDirectory();
        }

        protected virtual void ChangeDirectory(string message = null, Action callback = null)
        {
            if (string.IsNullOrEmpty(message)) message = Resx.ChangingDirectory;
            ProgressMessage = message + Strings.DotDotDot;
            IsBusy = true;
            ExecuteCancelCommand();

            WorkHandler.Run(() => ChangeDirectoryInner(CurrentFolder.Path),
                result =>
                {
                    ChangeDirectoryCallback(result);
                    if (callback != null) callback.Invoke();
                },
                AsyncErrorCallback);
        }

        protected virtual IList<FileSystemItem> ChangeDirectoryInner(string selectedPath)
        {
            var list = FileManager.GetList(selectedPath);
            list.ForEach(item => TitleRecognizer.RecognizeType(item));
            return list;
        }

        protected virtual void ChangeDirectoryCallback(IList<FileSystemItem> result)
        {
            IsBusy = false;
            lock (_queueLock)
            {
                _queue = new Queue<FileSystemItem>();
                var sw = new Stopwatch();
                sw.Start();
                foreach (var item in result.Where(item => item.TitleType != TitleType.Unknown && !TitleRecognizer.MergeWithCachedEntry(item)))
                {
                    _queue.Enqueue(item);
                }
                sw.Stop();
                Debug.WriteLine("[CD] Enqueued {0}: {1}", _queue.Count, sw.Elapsed);

                if (CurrentFolder.Type != ItemType.Drive)
                {
                    result.Insert(0, new FileSystemItem
                        {
                            Name = Strings.UpDirectory,
                            Type = CurrentFolder.Type,
                            Date = CurrentFolder.Date,
                            Path = CurrentFolder.Path,
                            Thumbnail = ResourceManager.GetContentByteArray("/Resources/up.png")
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
        }

        #endregion

        #region OpenStfsPackageCommand

        public DelegateCommand<OpenStfsPackageMode> OpenStfsPackageCommand { get; private set; }

        private bool CanExecuteOpenStfsPackageCommand(OpenStfsPackageMode mode)
        {
            //TODO: Remove IsProfile once STFS detection is implemented
            return CurrentRow != null && CurrentRow.IsProfile && !CurrentRow.IsLocked;
        }

        private void ExecuteOpenStfsPackageCommand(OpenStfsPackageMode mode)
        {
            ProgressMessage = string.Format("{0} {1}...", Resx.OpeningProfile, CurrentRow.ComputedName);
            IsBusy = true;
            WorkHandler.Run(() => OpenStfsPackage(CurrentRow.Model), b => OpenStfsPackageCallback(b, mode), AsyncErrorCallback);
        }

        protected virtual string GetStfsPackagePath(CacheComplexKey cacheKey, CacheEntry<FileSystemItem> cacheEntry)
        {
            if (cacheEntry.TempFilePath == null)
            {
                throw new ApplicationException(string.Format("Temp file for entry {0} not found.", cacheKey.Item.Path));
            }
            return cacheEntry.TempFilePath;
        }

        private BinaryContent OpenStfsPackage(FileSystemItem item)
        {
            if (item.IsLocked) throw new ApplicationException(item.LockMessage);

            var contentType = item.ContentType;
            CacheComplexKey cacheKey;
            var cacheEntry = TitleRecognizer.GetCacheEntry(item, out cacheKey);
            if (cacheEntry == null)
            {
                cacheEntry = TitleRecognizer.RecognizeTitle(item);
                if (cacheEntry == null)
                {
                    throw new ApplicationException(string.Format("Item cannot be recognized anymore: {0}", cacheKey.Item.Path));
                }
            }

            var path = GetStfsPackagePath(cacheKey, cacheEntry);
            return new BinaryContent(item.Path, path, File.ReadAllBytes(path), contentType);
        }

        private void OpenStfsPackageCallback(BinaryContent content, OpenStfsPackageMode mode)
        {
            PaneViewModelBase stfs;
            var data = new LoadDataAsyncParameters(Settings.Clone("/"), content);
            switch (mode)
            {
                case OpenStfsPackageMode.Browsing:
                    stfs = Container.Resolve<StfsPackageContentViewModel>();
                    break;
                case OpenStfsPackageMode.Repair:
                    stfs = Container.Resolve<ProfileRebuilderViewModel>();
                    break;
                default:
                    throw new NotSupportedException("Invalid mode: " + mode);
            }
            stfs.LoadDataAsync(LoadCommand.Load, data, OpenStfsPackageSuccess, OpenStfsPackageError);
        }

        private void OpenStfsPackageSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            EventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void OpenStfsPackageError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            WindowManager.ShowMessage(Resx.OpenFailed, string.Format("{0}: {1}", string.Format(Resx.CantOpenFile, CurrentRow.ComputedName), exception.Message));
        }

        #endregion

        #region OpenCompressedFileCommand

        public DelegateCommand OpenCompressedFileCommand { get; private set; }

        protected virtual bool CanExecuteOpenCompressedFileCommand()
        {
            return CurrentRow != null && CurrentRow.IsCompressedFile;
        }

        private void ExecuteOpenCompressedFileCommand()
        {
            ProgressMessage = string.Format("{0} {1}...", Resx.OpeningArchive, CurrentRow.ComputedName);
            IsBusy = true;
            WorkHandler.Run(() => OpenCompressedFile(CurrentRow.Model), OpenCompressedFileCallback, AsyncErrorCallback);
        }

        private string OpenCompressedFile(FileSystemItem item)
        {
            return item.Path;
        }

        private void OpenCompressedFileCallback(string path)
        {
            var archive = Container.Resolve<CompressedFileContentViewModel>();
            archive.LoadDataAsync(LoadCommand.Load, new LoadDataAsyncParameters(Settings.Clone("/"), path), OpenCompressedFileSuccess, OpenCompressedFileError);
        }

        private void OpenCompressedFileSuccess(PaneViewModelBase pane)
        {
            IsBusy = false;
            EventAggregator.GetEvent<OpenNestedPaneEvent>().Publish(new OpenNestedPaneEventArgs(this, pane));
        }

        private void OpenCompressedFileError(PaneViewModelBase pane, Exception exception)
        {
            IsBusy = false;
            WindowManager.ShowMessage(Resx.OpenFailed, string.Format("{0}: {1}", string.Format(Resx.CantOpenFile, CurrentRow.ComputedName), exception.Message));
        }

        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<bool> CalculateSizeCommand { get; private set; }
        private Queue<FileSystemItemViewModel> _calculationQueue;
        private bool _calculationIsRunning;
        private bool _calculationIsAborted;

        private void ExecuteCalculateSizeCommand(bool calculateAll)
        {
            if (calculateAll)
            {
                if (_calculationQueue == null) _calculationQueue = new Queue<FileSystemItemViewModel>();
                foreach (var item in Items.Where(item => item.Type == ItemType.Directory && !item.IsUpDirectory && !_calculationQueue.Contains(item)))
                {
                    _calculationQueue.Enqueue(item);
                    item.IsRefreshing = true;
                }
            } 
            else if (CurrentRow.Type == ItemType.Directory)
            {
                if (_calculationQueue == null) _calculationQueue = new Queue<FileSystemItemViewModel>();
                if (!_calculationQueue.Contains(CurrentRow))
                {
                    _calculationQueue.Enqueue(CurrentRow);
                    CurrentRow.IsRefreshing = true;
                }
            }

            if (_calculationQueue == null || _calculationQueue.Count <= 0 || _calculationIsRunning) return;

            _calculationIsRunning = true;
            _calculationIsAborted = false;
            WorkHandler.Run(CalculateSize, CalculateSizeCallback, AsyncErrorCallback);
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
            if (_calculationIsAborted) return 0;

            IList<FileSystemItem> list;
            try
            {
                list = FileManager.GetList(path);
            }
            catch
            {
                return 0;
            }
            return list.Where(item => item.Type == ItemType.File).Sum(fi => fi.Size.HasValue ? fi.Size.Value : 0)
                 + list.Where(item => item.Type == ItemType.Directory).Sum(di => CalculateSize(string.Format("{0}{1}/", path, di.Name)));
        }

        private void CalculateSizeCallback(long size)
        {
            lock (_calculationQueue)
            {
                var item = _calculationQueue.Dequeue();
                item.Size = size;
                item.IsRefreshing = false;
                if (!_calculationIsAborted && _calculationQueue.Count > 0)
                {
                    WorkHandler.Run(CalculateSize, CalculateSizeCallback, AsyncErrorCallback);
                }
                else
                {
                    _calculationQueue = null;
                    _calculationIsRunning = false;
                }
            }
            NotifyPropertyChanged(SIZEINFO);
        }

        private void CalculateSizeAbort()
        {
            if (!_calculationIsRunning) return;
            _calculationIsAborted = true;
            lock (_calculationQueue)
            {
                var item = _calculationQueue.Dequeue();
                _calculationQueue.Clear();
                _calculationQueue.Enqueue(item);
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
            Settings.SortByField = column.SortMemberPath;
            Settings.SortDirection = column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            var selection = CurrentRow;
            SortContent();
            CurrentRow = selection;
            column.SortDirection = Settings.SortDirection;
        }

        private void SortContent()
        {
            SortContent(Items);
        }

        private void SortContent(IEnumerable<FileSystemItemViewModel> content)
        {
            if (content == null) return;

            var list = content.OrderByDescending(p => p.Type).ThenByProperty(Settings.SortByField, Settings.SortDirection).ToList();
            var up = list.FirstOrDefault(item => item.IsUpDirectory);
            if (up != null)
            {
                list.Remove(up);
                list.Insert(0, up);
            }

            var currentRow = CurrentRow;
            var oldItems = Items.ToList();
            Items.Clear();
            Items.AddRange(list);
            EventAggregator.GetEvent<FileListPaneViewModelItemsChangedEvent>()
                           .Publish(new FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction.Replace, list, oldItems, this));
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

        #region InvertSelectionCommand

        public DelegateCommand<EventInformation<EventArgs>> InvertSelectionCommand { get; private set; }

        private void ExecuteInvertSelectionCommand(EventInformation<EventArgs> cmdParam)
        {
            Items.Where(row => !row.IsUpDirectory).ForEach(item => item.IsSelected = !item.IsSelected);
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
            return HasValidSelection;
        }

        private void ExecuteRefreshTitleCommand()
        {
            var selection = SelectedItems.Any() ? SelectedItems.Select(i => i.Model).ToList() : new List<FileSystemItem> { CurrentRow.Model };
            selection.ForEach(TitleRecognizer.ThrowCache);

            lock (_queueLock)
            {
                if (_queue != null)
                {
                    selection.Where(i => !_queue.Contains(i)).ForEach(_queue.Enqueue);
                    var item = _queue.Peek();
                    ProgressMessage = string.Format(Resx.RecognizingItem, item.Name, _queue.Count - 1);
                }
                else
                {
                    _queue = new Queue<FileSystemItem>(selection);
                    IsBusy = true;
                    RecognitionStart();
                }
            }
        }

        #endregion

        #region RecognizeFromProfileCommand

        public DelegateCommand RecognizeFromProfileCommand { get; private set; }

        private bool CanExecuteRecognizeFromProfileCommand()
        {
            return CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.IsProfile && !CurrentRow.IsLocked;
        }

        private void ExecuteRecognizeFromProfileCommand()
        {
            IsBusy = true;
            ProgressMessage = Resx.ScanningProfile + Strings.DotDotDot;
            WorkHandler.Run(RecognizeFromProfile, RecognizeFromProfileCallback, AsyncErrorCallback);
        }

        private int RecognizeFromProfile()
        {
            var cacheEntry = TitleRecognizer.GetCacheEntry(CurrentRow.Model);
            if (cacheEntry == null)
            {
                cacheEntry = TitleRecognizer.RecognizeTitle(CurrentRow.Model);
                if (cacheEntry == null || string.IsNullOrEmpty(cacheEntry.TempFilePath)) return -1;
            }

            var i = 0;
            var stfs = ModelFactory.GetModel<StfsPackage>(cacheEntry.TempFilePath);
            stfs.ExtractContent();
            stfs.ProfileInfo.TitlesPlayed.ForEach(g =>
                                                      {
                                                          var game = stfs.Games.Values.FirstOrDefault(gg => gg.TitleId == g.TitleCode);
                                                          if (game == null) return;
                                                          var gameCacheEntry = TitleRecognizer.GetCacheEntry(g.TitleCode);
                                                          if (gameCacheEntry != null && gameCacheEntry.Expiration == null) return;
                                                          var item = new FileSystemItem
                                                                         {
                                                                             Name = g.TitleCode,
                                                                             Title = g.TitleName,
                                                                             Type = ItemType.Directory,
                                                                             TitleType = TitleType.Game,
                                                                             Thumbnail = game.Thumbnail
                                                                         };
                                                          TitleRecognizer.UpdateCache(item);
                                                          i++;
                                                      });
            return i;
        }

        private void RecognizeFromProfileCallback(int count)
        {
            IsBusy = false;
            var message = count < 1 ? Resx.NoNewTitlesFound : string.Format(count > 1 ? Resx.NewTitleFoundPlural : Resx.NewTitleFoundSingular, count);
            WindowManager.ShowMessage(Resx.TitleRecognition, message);
        }

        #endregion

        #region CopyTitleIdToClipboardCommand

        public DelegateCommand CopyTitleIdToClipboardCommand { get; private set; }

        private bool CanExecuteCopyTitleIdToClipboardCommand()
        {
            return CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.TitleType == TitleType.Game;
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
            return CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.TitleType == TitleType.Game;
        }

        private void ExecuteSearchGoogleCommand()
        {
            Web.Browse(string.Format("http://www.google.com/#q={0}", CurrentRow.Name));
        }

        #endregion

        #region SearchGoogleCommand

        public DelegateCommand SaveThumbnailCommand { get; private set; }

        private bool CanExecuteSaveThumbnailCommand()
        {
            return CurrentRow != null && CurrentRow.HasThumbnail;
        }

        private void ExecuteSaveThumbnailCommand()
        {
            var dialog = new SaveFileDialog
                {
                    Filter = "PNG (*.PNG)|*.png", 
                    FileName = CurrentRow.Name
                };
            if (dialog.ShowDialog() == true)
            {
                using (var stream = dialog.OpenFile())
                {
                    var bytes = CurrentRow.Model.Thumbnail;
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }
        }

        #endregion

        #region RenameCommand

        public DelegateCommand<object> RenameTitleCommand { get; private set; }

        private bool CanExecuteRenameTitleCommand(object cmdParam)
        {
            return GlobalVariables.DataGridSupportsRenaming == true && CurrentRow != null && !CurrentRow.IsUpDirectory && CurrentRow.IsCached;
        }

        private void ExecuteRenameTitleCommand(object cmdParam)
        {
            Rename(cmdParam, FileSystemItemViewModel.TITLE);
        }

        public DelegateCommand<object> RenameFileSystemItemCommand { get; private set; }

        private bool CanExecuteRenameFileSystemItemCommand(object cmdParam)
        {
            return GlobalVariables.DataGridSupportsRenaming == true && CurrentRow != null && !CurrentRow.IsUpDirectory && !IsReadOnly;
        }

        private void ExecuteRenameFileSystemItemCommand(object cmdParam)
        {
            Rename(cmdParam, FileSystemItemViewModel.NAME);
        }

        private void Rename(object cmdParam, string tag)
        {
            var grid = cmdParam as DataGrid;
            var row = grid != null ? grid.FindRowByValue(CurrentRow) : cmdParam as DataGridRow;
            if (row == null) return;
            if (grid == null) grid = row.FindAncestor<DataGrid>();
            var cell = row.FirstCell();
            cell.Tag = tag; //param for template selector
            grid.PreparingCellForEdit += GridOnPreparingCellForEdit;
            grid.CellEditEnding += GridOnCellEditEnding;
            grid.PreviewKeyDown += GridOnPreviewKeyDown;
            cell.IsEditing = true;
            IsInEditMode = true;
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
        }

        private void GridOnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            e.EditingElement.LostFocus += GridCellEditingElementOnLostFocus;
        }

        private void GridOnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsInEditMode = false;
            var content = (ContentPresenter)e.EditingElement;
            content.LostFocus -= GridCellEditingElementOnLostFocus;
            var grid = (DataGrid)sender;
            grid.CellEditEnding -= GridOnCellEditEnding;
            grid.PreviewKeyDown -= GridOnPreviewKeyDown;
            if (e.EditAction == DataGridEditAction.Cancel) return;

            var tag = (string)content.FindAncestor<DataGridCell>().Tag;
            var template = content.ContentTemplateSelector.SelectTemplate(null, content);
            var textBox = (TextBox)template.FindName("TitleEditBox", content);
            var newValue = textBox.Text;

            switch (tag)
            {
                case FileSystemItemViewModel.TITLE:
                    if (CurrentRow.Title != newValue)
                    {
                        CurrentRow.Title = newValue;
                        TitleRecognizer.UpdateCache(CurrentRow.Model);
                    }
                    break;
                case FileSystemItemViewModel.NAME:
                    if (CurrentRow.Name != newValue)
                    {
                        var newModel = FileManager.Rename(CurrentRow.Model.Path, newValue);
                        TitleRecognizer.RecognizeType(newModel);
                        TitleRecognizer.RecognizeTitle(newModel);
                        var newItem = new FileSystemItemViewModel(newModel);
                        Items.Replace(CurrentRow, newItem);
                        EventAggregator.GetEvent<FileListPaneViewModelItemsChangedEvent>()
                                       .Publish(new FileListPaneViewModelItemsChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, CurrentRow, this));
                        CurrentRow = newItem;
                    }
                    break;
                default:
                    throw new NotSupportedException("Something went wrong, property change not supported: " + tag);
            }
            SortContent();
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
        }

        private void GridOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            var grid = (DataGrid) sender;
            grid.CancelEdit();
        }

        private void GridCellEditingElementOnLostFocus(object sender, RoutedEventArgs e)
        {
            var cell = (ContentPresenter) sender;
            var grid = cell.FindAncestor<DataGrid>();
            grid.CancelEdit();
        }

        #endregion

        #region RefreshCommand

        public DelegateCommand RefreshCommand { get; private set; }

        private bool CanExecuteRefreshCommand()
        {
            return true;
        }

        private void ExecuteRefreshCommand()
        {
            Refresh();
        }

        public void Refresh()
        {
            Refresh(null);
        }

        public void Refresh(Action callback)
        {
            ChangeDirectory(Resx.RefreshingDirectory, callback);
        }

        #endregion

        #region UpCommand

        public DelegateCommand UpCommand { get; private set; }

        private bool CanExecuteUpCommand()
        {
            return true;
        }

        private void ExecuteUpCommand()
        {
            if (CurrentFolder == Drive)
            {
                if (CloseCommand != null) CloseCommand.Execute();
            }
            else
            {
                var up = UpDirectory();
                if (up == null) return;
                CurrentFolder = up;
                ChangeDirectory();
            }
        }

        private FileSystemItemViewModel UpDirectory()
        {
            try
            {
                var parentPath = CurrentFolder.Path.GetParentPath();
                if (FileManager.FolderExists(parentPath))
                {
                    var type = parentPath == Drive.Path ? ItemType.Drive : ItemType.Directory;
                    var folder = FileManager.GetItemInfo(parentPath, type, false);
                    return new FileSystemItemViewModel(folder);
                }

                WindowManager.ShowMessage(Resx.IOError, string.Format(Resx.ItemNotExistsOnPath, parentPath));
                return Drive;
            }
            catch (Exception ex)
            {
                AsyncErrorCallback(ex);
                return null;
            }
        }

        #endregion

        #region CancelCommand

        public DelegateCommand CancelCommand { get; private set; }

        private void ExecuteCancelCommand()
        {
            if (_calculationIsRunning) CalculateSizeAbort();
        }

        #endregion

        #region CloseCommand

        public DelegateCommand CloseCommand { get; protected set; }

        #endregion

        #region SelectDriveByInitialLetterCommand

        public DelegateCommand<EventInformation<KeyEventArgs>> SelectDriveByInitialLetterCommand { get; protected set; }

        public void ExecuteSelectDriveByInitialLetterCommand(EventInformation<KeyEventArgs> e)
        {
            if (e.EventArgs.Key < Key.A || e.EventArgs.Key > Key.Z) return;
            var key = e.EventArgs.Key.ToString();
            var drive = Drives.FirstOrDefault(d => d.Name.StartsWith(key, StringComparison.InvariantCultureIgnoreCase));
            if (drive == null) return;
            e.EventArgs.Handled = true;
            Drive = drive;
        }

        #endregion

        protected FileListPaneViewModelBase()
        {
            FileManager = Container.Resolve<T>();
            UserSettings = Container.Resolve<IUserSettings>();
            TitleRecognizer = Container.Resolve<ITitleRecognizer>(new ParameterOverride("fileManager", FileManager));

            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand, CanExecuteChangeDirectoryCommand);
            OpenStfsPackageCommand = new DelegateCommand<OpenStfsPackageMode>(ExecuteOpenStfsPackageCommand, CanExecuteOpenStfsPackageCommand);
            OpenCompressedFileCommand = new DelegateCommand(ExecuteOpenCompressedFileCommand, CanExecuteOpenCompressedFileCommand);
            CalculateSizeCommand = new DelegateCommand<bool>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            ToggleSelectionCommand = new DelegateCommand<ToggleSelectionMode>(ExecuteToggleSelectionCommand);
            SelectAllCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteSelectAllCommand);
            InvertSelectionCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteInvertSelectionCommand);
            MouseSelectionCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteMouseSelectionCommand);
            GoToFirstCommand = new DelegateCommand<bool>(ExecuteGoToFirstCommand, CanExecuteGoToFirstCommand);
            GoToLastCommand = new DelegateCommand<bool>(ExecuteGoToLastCommand, CanExecuteGoToLastCommand);
            RefreshTitleCommand = new DelegateCommand(ExecuteRefreshTitleCommand, CanExecuteRefreshTitleCommand);
            RecognizeFromProfileCommand = new DelegateCommand(ExecuteRecognizeFromProfileCommand, CanExecuteRecognizeFromProfileCommand);
            CopyTitleIdToClipboardCommand = new DelegateCommand(ExecuteCopyTitleIdToClipboardCommand, CanExecuteCopyTitleIdToClipboardCommand);
            SearchGoogleCommand = new DelegateCommand(ExecuteSearchGoogleCommand, CanExecuteSearchGoogleCommand);
            SaveThumbnailCommand = new DelegateCommand(ExecuteSaveThumbnailCommand, CanExecuteSaveThumbnailCommand);
            RenameTitleCommand = new DelegateCommand<object>(ExecuteRenameTitleCommand, CanExecuteRenameTitleCommand);
            RenameFileSystemItemCommand = new DelegateCommand<object>(ExecuteRenameFileSystemItemCommand, CanExecuteRenameFileSystemItemCommand);
            RefreshCommand = new DelegateCommand(ExecuteRefreshCommand, CanExecuteRefreshCommand);
            UpCommand = new DelegateCommand(ExecuteUpCommand, CanExecuteUpCommand);
            CancelCommand = new DelegateCommand(ExecuteCancelCommand);
            SelectDriveByInitialLetterCommand = new DelegateCommand<EventInformation<KeyEventArgs>>(ExecuteSelectDriveByInitialLetterCommand);

            Items = new ObservableCollection<FileSystemItemViewModel>();

            EventAggregator.GetEvent<TransferProgressChangedEvent>().Subscribe(OnTransferProgressChanged);
        }

        public abstract string GetTargetPath(string path);
        public void Abort()
        {
            FileManager.Abort();
        }

        protected void Initialize()
        {
            Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
        }

        public virtual void FinishTransferAsSource()
        {
        }

        public virtual void FinishTransferAsTarget()
        {
        }

        public override void Dispose()
        {
            EventAggregator.GetEvent<TransferProgressChangedEvent>().Unsubscribe(OnTransferProgressChanged);
            if (CurrentFolder != null) Settings.Directory = CurrentFolder.FullPath;
            base.Dispose();
        }

        public override void SetActive()
        {
            base.SetActive();
            if (CurrentRow != null)
            {
                var x = Items.FirstOrDefault(item => item.Path == CurrentRow.Path);
                CurrentRow = x;
                return;
            }
            CurrentRow = Items.FirstOrDefault();
            RaiseCanExecuteChanges();
        }

        public FileExistenceInfo FileExists(string path)
        {
            return FileManager.FileExists(path);
        }

        public virtual TransferResult Delete(FileSystemItem item)
        {
            if (item.Type == ItemType.File)
            {
                FileManager.DeleteFile(item.Path);
            }
            else
            {
                FileManager.DeleteFolder(item.Path);
            }
            return TransferResult.Ok;
        }

        public virtual TransferResult CreateFolder(string path)
        {
            if (FileManager.FolderExists(path)) return TransferResult.Skipped;
            FileManager.CreateFolder(path);
            return TransferResult.Ok;
        }

        private bool IsDriveAccessible(FileSystemItemViewModel drive)
        {
            try
            {
                return FileManager.DriveIsReady(drive.Path);
            }
            catch (Exception ex)
            {
                AsyncErrorCallback(ex);
                return false;
            }
        }

        protected virtual void ChangeDrive()
        {
            CurrentRow = null;
            if (PathCache.ContainsKey(Drive))
            {
                var path = PathCache[Drive];
                var clearPath = new Regex(@"^(.*)[\\/].*(:[\\/]).*$");
                path = clearPath.Replace(path, "$1");
                var model = FileManager.GetItemInfo(path);
                if (path == Drive.Path) model.Type = ItemType.Drive;
                CurrentFolder = model != null ? new FileSystemItemViewModel(model) : Drive;
            }
            else
            {
                CurrentFolder = Drive;
            }
            
            ChangeDirectoryCommand.Execute(null);
        }

        public override void RaiseCanExecuteChanges()
        {
            base.RaiseCanExecuteChanges();
            ChangeDirectoryCommand.RaiseCanExecuteChanged();
            OpenStfsPackageCommand.RaiseCanExecuteChanged();
            CalculateSizeCommand.RaiseCanExecuteChanged();
            RefreshTitleCommand.RaiseCanExecuteChanged();
            RecognizeFromProfileCommand.RaiseCanExecuteChanged();
            CopyTitleIdToClipboardCommand.RaiseCanExecuteChanged();
            SearchGoogleCommand.RaiseCanExecuteChanged();
            SaveThumbnailCommand.RaiseCanExecuteChanged();
            RenameTitleCommand.RaiseCanExecuteChanged();
            RenameFileSystemItemCommand.RaiseCanExecuteChanged();
        }

        public void GetItemViewModel(string itemPath)
        {
            var listedItem = Items.FirstOrDefault(item => item.Path == itemPath);
            if (listedItem != null)
            {
                PublishItemViewModel(listedItem);
                return;
            }

            WorkHandler.Run(() => FileManager.GetItemInfo(itemPath, ItemType.File), item =>
                {
                    if (item == null) throw new ApplicationException(string.Format(Resx.ItemNotExistsOnPath, itemPath));
                    var vm = new FileSystemItemViewModel(item);
                    RecognitionInner(item, i => PublishItemViewModel(vm), null);
                });
        }

        private void RecognitionStart()
        {
            lock (_queueLock)
            {
                if (_queue.Count > 0)
                {
                    var item = _queue.Peek();
                    ProgressMessage = string.Format(Resx.RecognizingItem + Strings.DotDotDot, item.Name, _queue.Count - 1);
                    RecognitionInner(item, RecognitionSuccess, RecognitionError);
                }
                else
                {
                    RecognitionFinish();
                }
            }
        }

        private void RecognitionInner(FileSystemItem item, Action<FileSystemItem> success, Action<Exception> error)
        {
            WorkHandler.Run(() =>
            {
                TitleRecognizer.RecognizeTitle(item);
                return item;
            }, 
            success,
            error);
        }

        private void RecognitionSuccess(FileSystemItem item)
        {
            Items.Single(i => i.Model == item).NotifyModelChanges();
            lock (_queueLock)
            {
                _queue.Dequeue();
            }
            RecognitionStart();
        }

        private void RecognitionError(Exception exception)
        {
            lock (_queueLock)
            {
                _queue.Dequeue();
            }
            RecognitionStart();
        }

        private void RecognitionFinish()
        {
            lock (_queueLock)
            {
                _queue = null;
            }
            SortContent();
            IsBusy = false;
        }

        private void PublishItemViewModel(ViewModelBase vm)
        {
            EventAggregator.GetEvent<ViewModelGeneratedEvent>().Publish(new ViewModelGeneratedEventArgs(vm));
        }

        public Queue<QueueItem> PopulateQueue(FileOperation action)
        {
            if (!SelectedItems.Any()) CurrentRow.IsSelected = true;
            var direction = action == FileOperation.Delete
                                ? TreeTraversalDirection.Upward
                                : TreeTraversalDirection.Downward;
            var res = PopulateQueue(SelectedItems.Select(vm => vm.Model), direction, action);
            SelectedItems.ForEach(item => item.NotifyModelChanges());
            var queue = new Queue<QueueItem>();
            res.ForEach(queue.Enqueue);
            return queue;
        }

        private List<QueueItem> PopulateQueue(IEnumerable<FileSystemItem> items, TreeTraversalDirection direction, FileOperation action)
        {
            var result = new List<QueueItem>();
            foreach (var item in items)
            {
                if (direction == TreeTraversalDirection.Downward) result.Add(new QueueItem(item, action));
                if (item.Type == ItemType.Directory) //TODO: Link?
                {
                    var sub = PopulateQueue(ChangeDirectoryInner(item.Path), direction, action);
                    item.Size = sub.Where(i => i.FileSystemItem.Type == ItemType.File).Sum(i => i.FileSystemItem.Size ?? 0);
                    result.AddRange(sub);
                }
                if (direction == TreeTraversalDirection.Upward) result.Add(new QueueItem(item, action));
            }
            return result;
        }

        protected virtual void AsyncErrorCallback(Exception ex)
        {
            _calculationIsRunning = false;
            IsBusy = false;
            EventAggregator.GetEvent<ShowCorrespondingErrorEvent>().Publish(new ShowCorrespondingErrorEventArgs(ex, false));
        }

        public virtual Stream GetStream(string path, FileMode mode, FileAccess access, long startPosition)
        {
            return FileManager.GetStream(path, mode, access, startPosition);
        }

        public virtual bool CopyStream(FileSystemItem item, Stream stream, long startPosition = 0, long? byteLimit = null)
        {
            return FileManager.CopyTo(item, stream, startPosition, byteLimit);
        }

        private void OnTransferProgressChanged(TransferProgressChangedEventArgs args)
        {
            UIThread.Run(() =>
                {
                    if (string.IsNullOrEmpty(ProgressMessage) || ProgressMessage.StartsWith(Resx.ChangingDirectory)) return;
                    var r = new Regex(@" \([0-9]+%\)$");
                    ProgressMessage = r.Replace(ProgressMessage, string.Empty);
                    ProgressMessage += string.Format(" ({0}%)", args.Percentage);
                });
        }

    }
}