using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Practices.ObjectBuilder2;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.ViewModels;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Fizzler.Systems.HtmlAgilityPack;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Extensions;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FreestyleDatabaseCheckerViewModel : CommonViewModelBase, IProgressViewModel, ITreeSelectionViewModel
    {
        private int _itemsCount;
        private int _itemsChecked;

        private readonly ITransferManagerViewModel _transferManager;

        private readonly FtpContentViewModel _parent;
        public IViewModel Parent
        {
            get { return _parent; }
        }

        private const string HASMISSINGFOLDERS = "HasMissingFolders";
        public bool HasMissingFolders
        {
            get { return MissingFolders.Any(); }
        }

        private const string MISSINGFOLDERSCOUNT = "MissingFoldersCount";
        public int MissingFoldersCount
        {
            get { return MissingFolders.Count; }
        }

        private const string MISSINGFOLDERS = "MissingFolders";
        private ObservableCollection<FileSystemItemViewModel> _missingFolders;
        public ObservableCollection<FileSystemItemViewModel> MissingFolders
        {
            get { return _missingFolders; }
            set
            {
                _missingFolders = value;
                NotifyPropertyChanged(MISSINGFOLDERS);
                NotifyPropertyChanged(HASMISSINGFOLDERS);
                NotifyPropertyChanged(MISSINGFOLDERSCOUNT);
            }
        }

        private const string HASMISSINGENTRIES = "HasMissingEntries";
        public bool HasMissingEntries
        {
            get { return MissingEntries.Any(); }
        }

        private const string MISSINGENTRIESCOUNT = "MissingEntriesCount";
        public int MissingEntriesCount
        {
            get { return MissingEntries.Count; }
        }

        private const string MISSINGENTRIES = "MissingEntries";
        private ObservableCollection<FileSystemItemViewModel> _missingEntries;
        public ObservableCollection<FileSystemItemViewModel> MissingEntries
        {
            get { return _missingEntries; }
            set
            {
                _missingEntries = value;
                NotifyPropertyChanged(MISSINGENTRIES);
                NotifyPropertyChanged(HASMISSINGENTRIES);
                NotifyPropertyChanged(MISSINGENTRIESCOUNT);
            }
        }

        public string TreeSelectionTitle
        {
            get { return Resx.Cleanup; }
        }

        public string TreeSelectionDescription
        {
            get { return Resx.SelectTheItemsYouWantToRemove + Strings.Colon; }
        }

        private const string SELECTIONTREE = "SelectionTree";
        private ObservableCollection<TreeItemViewModel> _selectionTree;
        public ObservableCollection<TreeItemViewModel> SelectionTree
        {
            get { return _selectionTree; }
            set { _selectionTree = value; NotifyPropertyChanged(SELECTIONTREE); }
        }

        private const string PROGRESSDIALOGTITLE = "ProgressDialogTitle";
        private readonly string _progressDialogTitle = Resx.FreestyleDatabaseCheck + " ({0}%)";
        public string ProgressDialogTitle
        {
            get { return string.Format(_progressDialogTitle, ProgressValue); }
        }

        private const string PROGRESSMESSAGE = "ProgressMessage";
        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            private set { _progressMessage = value; NotifyPropertyChanged(PROGRESSMESSAGE); }
        }

        private const string PROGRESSVALUE = "ProgressValue";
        public int ProgressValue
        {
            get { return _itemsCount == 0 ? 0 : _itemsChecked * 100 / _itemsCount; }
        }

        private const string PROGRESSVALUEDOUBLE = "ProgressValueDouble";
        public double ProgressValueDouble
        {
            get { return (double)ProgressValue / 100; }
        }

        private const string ISINDETERMINE = "IsIndetermine";
        private bool _isIndetermine;
        public bool IsIndetermine
        {
            get { return _isIndetermine; }
            private set { _isIndetermine = value; NotifyPropertyChanged(ISINDETERMINE); }
        }

        #region CloseCommand

        public DelegateCommand CloseCommand { get; private set; }

        public void ExecuteCloseCommand()
        {
            WindowManager.CloseWindowOf<FreestyleDatabaseCheckerViewModel>();
        }

        #endregion

        #region CleanUpCommand

        public DelegateCommand CleanUpCommand { get; private set; }

        private void ExecuteCleanUpCommand()
        {
            var tree = new Tree<FileSystemItem>();
            foreach (var entry in MissingEntries)
            {
                tree.Insert(entry.Path, entry.Model, CreateEmptyParentNode);
                foreach (var item in (IList<FileSystemItem>)entry.Content)
                {
                    tree.Insert(item.Path, item, CreateEmptyParentNode);
                }
            }
            SelectionTree = new ObservableCollection<TreeItemViewModel>();
            WrapTreeIntoViewModels(tree, SelectionTree, null);
            if (!WindowManager.ShowTreeSelectorDialog(this)) return;

            var selection = new List<FileSystemItem>();
            GetSelectionFromTree(SelectionTree, selection);
            _transferManager.Delete(_parent, selection);
            ExecuteCloseCommand();
        }

        private FileSystemItem CreateEmptyParentNode(string name)
        {
            return new FileSystemItem
                       {
                           Name = name,
                           Type = ItemType.Directory
                       };
        }

        private void WrapTreeIntoViewModels(TreeItem<FileSystemItem> tree, ObservableCollection<TreeItemViewModel> children, TreeItemViewModel parent)
        {
            foreach (var treeItem in tree.Children.Values)
            {
                var icon = treeItem.Content.Type == ItemType.Directory ? "/Resources/folder.png" : "/Resources/file.png";
                var vm = new TreeItemViewModel(parent)
                             {
                                 Name = treeItem.Name,
                                 Title = treeItem.Content.Title,
                                 Content = treeItem.Content,
                                 Thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(treeItem.Content.Thumbnail ?? ResourceManager.GetContentByteArray(icon)),
                                 IsChecked = true,
                                 IsDirectory = treeItem.Content.Type == ItemType.Directory
                             };
                children.Add(vm);
                WrapTreeIntoViewModels(treeItem, vm.Children, vm);
            }
        }

        private void GetSelectionFromTree(ObservableCollection<TreeItemViewModel> tree, IList<FileSystemItem> selection)
        {
            foreach (var treeItem in tree)
            {
                if (treeItem.Children != null && treeItem.Children.Any())
                {
                    var content = treeItem.Content as FileSystemItem;
                    if (content != null && content.TitleType == TitleType.Game && treeItem.IsChecked == true)
                    {
                        selection.Add(content);
                    }
                    else
                    {
                        GetSelectionFromTree(treeItem.Children, selection);
                    }
                }
                else if (treeItem.IsChecked == true) selection.Add(treeItem.Content as FileSystemItem);
            }
        }

        #endregion

        public FreestyleDatabaseCheckerViewModel(ITransferManagerViewModel transferManager, FtpContentViewModel parent)
        {
            _transferManager = transferManager;
            _parent = parent;
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            CleanUpCommand = new DelegateCommand(ExecuteCleanUpCommand);
        }

        public void Check()
        {
            IsBusy = true;
            ProgressMessage = Resx.GettingData + Strings.DotDotDot;
            IsIndetermine = true;
            var missingFolders = new List<FileSystemItem>();
            var missingEntries = new Dictionary<FileSystemItem, IList<FileSystemItem>>();
            this.NotifyProgressStarted();

            WorkHandler.Run(() =>
            {
                
                var gameFolders = new List<FileSystemItem>();
                foreach (var drive in _parent.Drives)
                {
                    try
                    {
                        gameFolders.AddRange(_parent.GetList(drive.Path + "Content/0000000000000000"));
                    }
                    catch
                    {
                    }
                }

                var html = new HtmlDocument();
                IEnumerable<HtmlNode> rows;
                switch (_parent.ServerType)
                {
                    case FtpServerType.F3:
                        html.LoadHtml(_parent.HttpGetString("gettable.html?name=ContentItems", Encoding.UTF8));
                        rows = html.DocumentNode.QuerySelectorAll("table.GameContentHeader > tr").Skip(2);
                        break;
                    case FtpServerType.FSD:
                        html.LoadHtml(_parent.HttpPostString("gettable?name=ContentItems", "sessionid=" + _parent.HttpSessionId, Encoding.UTF8));
                        rows = html.DocumentNode.QuerySelectorAll("tr").Skip(1);
                        break;
                    default:
                        throw new NotSupportedException("Invalid server type: " + _parent.ServerType);
                }
                
                _itemsCount = rows.Count();
                UIThread.Run(() => IsIndetermine = false);
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td")
                                   .Select((c, i) => new { key = (FsdContentItemProperty)i, value = c.InnerText.Trim() })
                                   .ToDictionary(k => k.key, k => k.value);

                    var titleIdInt = Int32.Parse(cells[FsdContentItemProperty.TitleId]);
                    if (titleIdInt != 0)
                    {
                        var titleId = titleIdInt.ToString("X");
                        gameFolders.RemoveAll(g => g.Name.Equals(titleId, StringComparison.InvariantCultureIgnoreCase));
                    }

                    var contentId = Int32.Parse(cells[FsdContentItemProperty.Id]);
                    var title = cells[FsdContentItemProperty.Name];
                    UIThread.Run(() =>
                    {
                        ProgressMessage = Resx.Checking + Strings.ColonSpace + title;
                        NotifyPropertyChanged(PROGRESSVALUE);
                        NotifyPropertyChanged(PROGRESSVALUEDOUBLE);
                        NotifyPropertyChanged(PROGRESSDIALOGTITLE);
                    });
                    _itemsChecked++;
                    var scanPathId = Int32.Parse(cells[FsdContentItemProperty.ScanPathId]);
                    if (!_parent.ScanFolders.ContainsKey(scanPathId))
                    {
                        //TODO: handle non-existent scan path
                        continue;
                    }
                    var f = _parent.ScanFolders[scanPathId];
                    var path = string.Format("/{0}{1}", f.Drive, cells[FsdContentItemProperty.Path].Replace("\\", "/"));
                    if (!_parent.FileExists(path))
                    {
                        missingFolders.Add(new FileSystemItem
                        {
                            Title = title,
                            Path = path,
                            Thumbnail = _parent.HttpGet(string.Format("assets/gameicon.png?contentid={0:X2}", contentId))
                        });
                    }
                }

                UIThread.Run(() => ProgressMessage = Resx.PleaseWait);

                foreach (var item in gameFolders)
                {
                    _parent.TitleRecognizer.RecognizeType(item);
                    if (item.TitleType != TitleType.Game) continue;
                    if (!_parent.TitleRecognizer.MergeWithCachedEntry(item)) _parent.TitleRecognizer.RecognizeTitle(item);
                    var content = _parent.GetList(item.Path);
                    long sum = 0;
                    content.ForEach(c =>
                                        {
                                            _parent.TitleRecognizer.RecognizeType(c);
                                            var size = _parent.CalculateSize(c.Path);
                                            c.Size = size;
                                            sum += size;
                                        });
                    item.Size = sum;
                    missingEntries.Add(item, content.OrderByDescending(c => c.Size).ToList());
                }

                return true;
            },
            result =>
            {
                IsBusy = false;
                ProgressMessage = string.Empty;
                MissingFolders = missingFolders.Select(m => new FileSystemItemViewModel(m)).ToObservableCollection();
                MissingEntries = missingEntries.Select(m => new FileSystemItemViewModel(m.Key) { Content = m.Value }).ToObservableCollection();
                this.NotifyProgressFinished();
                EventAggregator.GetEvent<FreestyleDatabaseCheckedEvent>().Publish(new FreestyleDatabaseCheckedEventArgs(this));
            },
            error =>
            {
                IsBusy = false;
                ProgressMessage = string.Empty;
                this.NotifyProgressFinished();
                //TODO: swallow error and pop up a "something went wrong" text?
                WindowManager.ShowErrorMessage(error);
            });
        }

    }
}