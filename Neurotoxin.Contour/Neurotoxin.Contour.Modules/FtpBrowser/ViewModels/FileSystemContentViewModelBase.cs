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
using System.Windows.Data;
using Neurotoxin.Contour.Core;
using Neurotoxin.Contour.Modules.FtpBrowser.Helpers;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Expression = System.Linq.Expressions.Expression;
using TextChangedEventArgs = Neurotoxin.Contour.Core.TextChangedEventArgs;
using Microsoft.Practices.Composite;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public abstract class FileSystemContentViewModelBase : ModuleViewModelBase
    {
        protected readonly BinaryFormatter BinaryFormatter;

        #region Properties

        private const string DRIVE = "Drive";
        private string _drive;
        public string Drive
        {
            get { return _drive; }
            set { _drive = value; NotifyPropertyChanged(DRIVE); }
        }

        private const string STACK = "Stack";
        private Stack<FileSystemItemViewModel> _stack;
        public Stack<FileSystemItemViewModel> Stack
        {
            get { return _stack; }
            set { _stack = value; NotifyPropertyChanged(STACK); }
        }

        private const string SELECTEDPATH = "SelectedPath";
        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set { _selectedPath = value; NotifyPropertyChanged(SELECTEDPATH); }
        }

        private const string CONTENT = "Content";
        private ObservableCollection<FileSystemItemViewModel> _content;
        public ObservableCollection<FileSystemItemViewModel> Content
        {
            get { return _content; }
            set { _content = value; NotifyPropertyChanged(CONTENT); }
        }

        private const string SELECTION = "Selection";
        private FileSystemItemViewModel _selection;
        public FileSystemItemViewModel Selection
        {
            get { return _selection; }
            set { _selection = value; NotifyPropertyChanged(SELECTION); }
        }

        private string _sortMemberPath;
        private ListSortDirection _listSortDirection;

        #endregion

        public override bool HasDirty()
        {
            throw new NotImplementedException();
        }

        protected override void ResetDirtyFlags()
        {
            throw new NotImplementedException();
        }

        public override bool IsDirty(string propertyName)
        {
            throw new NotImplementedException();
        }

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            var eventInformation = cmdParam as EventInformation<System.Windows.Input.MouseEventArgs>;
            if (eventInformation != null)
            {
                var e = eventInformation.EventArgs;
                var dataContext = ((FrameworkElement) e.OriginalSource).DataContext;
                if (!(dataContext is FileSystemItemViewModel) || Selection.Type == ItemType.File) return;
                SelectedPath = Selection.Path;
                if (Selection.Title == "[..]")
                {
                    Stack.Pop();
                }
                else
                {
                    Stack.Push(Selection);
                }
            }
            else
            {
                SelectedPath = (string) cmdParam;
            }
            LoadSubscribe();
            WorkerThread.Run(ChangeDirectory, ChangeDirectoryCallback);
        }

        protected abstract List<FileSystemItem> ChangeDirectory();

        protected virtual void ChangeDirectoryCallback(List<FileSystemItem> content)
        {
            SortContent(content.Select(c => new FileSystemItemViewModel
                {
                    Path = c.Path,
                    Title = c.Title,
                    TitleId = c.TitleId,
                    Thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(c.Thumbnail),
                    Size = c.Size,
                    Date = c.Date,
                    Type = c.Type
                }));

            IsInProgress = false;
            LogHelper.StatusBarChange -= LogHelperStatusBarChange;
            LogHelper.StatusBarMax -= LogHelperStatusBarMax;
            LogHelper.StatusBarText -= LogHelperStatusBarText;
            LoadingInfo = "Done.";
        }

        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<object> CalculateSizeCommand { get; private set; }

        private void ExecuteCalculateSizeCommand(object cmdParam)
        {
            LoadSubscribe();
            WorkerThread.Run(CalculateSize, CalculateSizeCallback);
        }

        private bool CanExecuteCalculateSizeCommand(object cmdParam)
        {
            return Selection != null;
        }

        private long CalculateSize()
        {
            return CalculateSize(Selection.Path);
        }

        protected abstract long CalculateSize(string path);

        protected virtual void CalculateSizeCallback(long size)
        {
            Selection.Size = size;
            IsInProgress = false;
            LogHelper.StatusBarChange -= LogHelperStatusBarChange;
            LogHelper.StatusBarMax -= LogHelperStatusBarMax;
            LogHelper.StatusBarText -= LogHelperStatusBarText;
            LoadingInfo = "Done.";
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
            SortContent(Content);
            column.SortDirection = _listSortDirection;
        }

        private void SortContent(IEnumerable<FileSystemItemViewModel> content)
        {
            Content = Content ?? new ObservableCollection<FileSystemItemViewModel>();
            if (_sortMemberPath == null)
            {
                Content.Clear();
                Content.AddRange(content);
                return;
            }

            var command = _listSortDirection == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var type = typeof(FileSystemItemViewModel);
            var property = type.GetProperty(_sortMemberPath);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new[] { type, property.PropertyType }, content.AsQueryable().Expression, Expression.Quote(orderByExpression));

            var collection = content.AsQueryable().Provider.CreateQuery<FileSystemItemViewModel>(resultExpression).ToList();
            var up = collection.FirstOrDefault(item => item.IsUpDirectory);
            if (up != null)
            {
                collection.Remove(up);
                collection.Insert(0, up);
            }

            Content.Clear();
            Content.AddRange(collection);
        }

        #endregion

        public FileSystemContentViewModelBase()
        {
            BinaryFormatter = new BinaryFormatter();
            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand);
            CalculateSizeCommand = new DelegateCommand<object>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
        }

        protected void LoadSubscribe()
        {
            IsInProgress = true;
            LoadingQueueLength = 1;
            LoadingProgress = 0;
            LogHelper.StatusBarChange += LogHelperStatusBarChange;
            LogHelper.StatusBarMax += LogHelperStatusBarMax;
            LogHelper.StatusBarText += LogHelperStatusBarText;
        }

        protected void LoadUnsubscribe()
        {
            IsInProgress = false;
            LogHelper.StatusBarChange -= LogHelperStatusBarChange;
            LogHelper.StatusBarMax -= LogHelperStatusBarMax;
            LogHelper.StatusBarText -= LogHelperStatusBarText;
        }

        private void LogHelperStatusBarChange(object sender, ValueChangedEventArgs e)
        {
            UIThread.BeginRun(() => LoadingProgress = e.NewValue);
        }

        private void LogHelperStatusBarMax(object sender, ValueChangedEventArgs e)
        {
            UIThread.BeginRun(() =>
                                  {
                                      LoadingQueueLength = e.NewValue;
                                      LoadingProgress = 0;
                                  });
        }

        private void LogHelperStatusBarText(object sender, TextChangedEventArgs e)
        {
            UIThread.BeginRun(() => LoadingInfo = e.Text);
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