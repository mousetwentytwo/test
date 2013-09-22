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
using Neurotoxin.Contour.Modules.FtpBrowser.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;
using Expression = System.Linq.Expressions.Expression;
using Microsoft.Practices.Composite;

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

        #region SetActiveCommand

        public DelegateCommand<object> SetActiveCommand { get; private set; }

        private void ExecuteSetActiveCommand(object cmdParam)
        {
            IsActive = true;
        }

        #endregion

        #region ChangeDirectoryCommand

        public DelegateCommand<object> ChangeDirectoryCommand { get; private set; }

        private void ExecuteChangeDirectoryCommand(object cmdParam)
        {
            var eventInformation = cmdParam as EventInformation<System.Windows.Input.MouseEventArgs>;
            var itemViewModel = cmdParam as FileSystemItemViewModel;
            if (eventInformation != null)
            {
                var e = eventInformation.EventArgs;
                var dataContext = ((FrameworkElement) e.OriginalSource).DataContext;
                if (!(dataContext is FileSystemItemViewModel) || Selection.Type == ItemType.File) return;
                itemViewModel = Selection;
            }

            if (itemViewModel != null) {
                SelectedPath = itemViewModel.Path;
                if (itemViewModel.Title == "[..]")
                    Stack.Pop();
                else
                    Stack.Push(itemViewModel);
            }
            else
            {
                //UNDONE: Fucks up the Stack!!!
                SelectedPath = (string) cmdParam;
            }
            Parent.IsInProgress = true;
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
                    Type = c.Type,
                    Subtype = c.Subtype
                }));
            //Selection = Content.FirstOrDefault();

            Parent.IsInProgress = false;
        }

        #endregion

        #region CalculateSizeCommand

        public DelegateCommand<object> CalculateSizeCommand { get; private set; }

        private void ExecuteCalculateSizeCommand(object cmdParam)
        {
            Parent.IsInProgress = true;
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
            Parent.IsInProgress = false;
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
            var selection = Selection;
            SortContent(Content);
            Selection = selection;
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

            var collection = content.AsQueryable().Provider.CreateQuery<FileSystemItemViewModel>(resultExpression).OrderByDescending(p => p.Type).ToList();
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

        public PaneViewModelBase(ModuleViewModelBase parent)
        {
            Parent = parent;
            BinaryFormatter = new BinaryFormatter();
            SetActiveCommand = new DelegateCommand<object>(ExecuteSetActiveCommand);
            ChangeDirectoryCommand = new DelegateCommand<object>(ExecuteChangeDirectoryCommand);
            CalculateSizeCommand = new DelegateCommand<object>(ExecuteCalculateSizeCommand, CanExecuteCalculateSizeCommand);
            SortingCommand = new DelegateCommand<EventInformation<DataGridSortingEventArgs>>(ExecuteSortingCommand);
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(e => { if (e.ActivePane != this) IsActive = false; });
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
            ChangeDirectoryCommand.Execute(SelectedPath);
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