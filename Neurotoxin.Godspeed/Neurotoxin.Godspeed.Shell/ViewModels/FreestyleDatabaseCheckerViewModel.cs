using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Fizzler.Systems.HtmlAgilityPack;
using Neurotoxin.Godspeed.Presentation.Extensions;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FreestyleDatabaseCheckerViewModel : ViewModelBase, IProgressViewModel
    {
        private int _itemsCount;
        private int _itemsChecked;
        private FtpContentViewModel _parent;

        private const string MISSINGENTRIES = "MissingEntries";
        private ObservableCollection<FileSystemItemViewModel> _missingEntries;
        public ObservableCollection<FileSystemItemViewModel> MissingEntries
        {
            get { return _missingEntries; }
            set { _missingEntries = value; NotifyPropertyChanged(MISSINGENTRIES); }
        }

        private const string PROGRESSDIALOGTITLE = "ProgressDialogTitle";
        private readonly string _progressDialogTitle = Resx.CheckFreestyleDatabase + " ({0}%)";
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

        public FreestyleDatabaseCheckerViewModel(FtpContentViewModel parent)
        {
            _parent = parent;
        }

        public void Check()
        {
            IsBusy = true;
            ProgressMessage = Resx.GettingData + Strings.DotDotDot;
            IsIndetermine = true;

            WorkHandler.Run(() =>
            {
                var missing = new List<FileSystemItem>();
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
                        missing.Add(new FileSystemItem
                        {
                            Title = title,
                            Path = path,
                            Thumbnail = _parent.HttpGet(string.Format("assets/gameicon.png?contentid={0:X2}", contentId))
                        });
                    }
                    _itemsChecked++;
                }
                return missing;
            },
            result =>
            {
                IsBusy = false;
                ProgressMessage = string.Empty;
                MissingEntries = result.Select(m => new FileSystemItemViewModel(m)).ToObservableCollection();
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