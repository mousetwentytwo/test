using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
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
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FreestyleDatabaseCheckerViewModel : CommonViewModelBase, IProgressViewModel, ITreeSelectionViewModel
    {
        private int _itemsCount;
        private int _itemsChecked;
        private FtpContentViewModel _parent;

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

        public event WorkFinishedEventHandler Finished;

        private void NotifyFinished()
        {
            var handler = Finished;
            if (handler != null) handler.Invoke(this);
        }

        public event EventHandler Close;

        private void NotifyClose()
        {
            var handler = Close;
            if (handler != null) handler.Invoke(this, new EventArgs());
        }

        #region CloseCommand

        public DelegateCommand CloseCommand { get; private set; }

        public void ExecuteCloseCommand()
        {
            NotifyClose();
        }

        #endregion

        #region CleanUpCommand

        public DelegateCommand CleanUpCommand { get; private set; }

        private void ExecuteCleanUpCommand()
        {
            var tree = new Tree<FileSystemItem>();
            foreach (var entry in MissingEntries.SelectMany(entry => (IList<FileSystemItem>)entry.Content))
            {
                tree.Insert(entry.Path, entry, name => new FileSystemItem
                                                           {
                                                               Name = name,
                                                               Type = ItemType.Directory
                                                           });
            }
            SelectionTree = new ObservableCollection<TreeItemViewModel>();
            WrapTreeIntoViewModel(tree, SelectionTree);
            if (WindowManager.ShowTreeSelectorDialog(this))
            {
                //TODO
            }
        }

        private static void WrapTreeIntoViewModel(TreeItem<FileSystemItem> tree, ObservableCollection<TreeItemViewModel> treeViewModel)
        {
            foreach (var treeItem in tree.Children.Values)
            {
                var vm = new TreeItemViewModel
                             {
                                 Name = treeItem.Name,
                                 IsSelected = true,
                                 IsDirectory = treeItem.Content.Type == ItemType.Directory,
                                 Children = new ObservableCollection<TreeItemViewModel>()
                             };
                treeViewModel.Add(vm);
                WrapTreeIntoViewModel(treeItem, vm.Children);
            }
        }

        #endregion

        public FreestyleDatabaseCheckerViewModel(FtpContentViewModel parent)
        {
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
                html.LoadHtml(_parent.HttpGetString("gettable.html?name=ContentItems", Encoding.UTF8));
                var rows = html.DocumentNode.QuerySelectorAll("table.GameContentHeader > tr").Skip(2);
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
                    var scanPathId = Int32.Parse(cells[FsdContentItemProperty.ScanPathId]);
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
                    _itemsChecked++;
                }

                UIThread.Run(() => ProgressMessage = Resx.PleaseWait + Strings.DotDotDot); //TODO: 'just a little bit longer' text

                foreach (var item in gameFolders)
                {
                    _parent.TitleRecognizer.RecognizeType(item);
                    if (item.TitleType != TitleType.Game) continue;
                    if (!_parent.TitleRecognizer.MergeWithCachedEntry(item)) _parent.TitleRecognizer.RecognizeTitle(item);
                    var content = _parent.GetList(item.Path);
                    content.ForEach(c =>
                                        {
                                            _parent.TitleRecognizer.RecognizeType(c);
                                            c.Size = _parent.CalculateSize(c.Path);
                                        });

                    item.Size = content.Sum(c => c.Size);
                    missingEntries.Add(item, content);
                }

                return true;
            },
            result =>
            {
                IsBusy = false;
                ProgressMessage = string.Empty;
                MissingFolders = missingFolders.Select(m => new FileSystemItemViewModel(m)).ToObservableCollection();
                MissingEntries = missingEntries.Select(m => new FileSystemItemViewModel(m.Key) { Content = m.Value }).ToObservableCollection();
                NotifyFinished();
            },
            error =>
            {
                IsBusy = false;
                ProgressMessage = string.Empty;
                NotifyFinished(); //TODO: different event or overload
                //TODO
            });
        }

    }
}