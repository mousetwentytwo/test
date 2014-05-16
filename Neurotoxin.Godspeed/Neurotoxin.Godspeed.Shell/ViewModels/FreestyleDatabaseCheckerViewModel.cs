using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;
using Fizzler.Systems.HtmlAgilityPack;
using Neurotoxin.Godspeed.Presentation.Extensions;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FreestyleDatabaseCheckerViewModel : ViewModelBase
    {
        private const string MISSINGENTRIES = "MissingEntries";
        private ObservableCollection<FileSystemItemViewModel> _missingEntries;
        public ObservableCollection<FileSystemItemViewModel> MissingEntries
        {
            get { return _missingEntries; }
            set { _missingEntries = value; NotifyPropertyChanged(MISSINGENTRIES); }
        }

        private const string PROGRESSMESSAGE = "ProgressMessage";
        private string _progressMessage;
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { _progressMessage = value; NotifyPropertyChanged(PROGRESSMESSAGE); }
        }

        public FreestyleDatabaseCheckerViewModel(FtpContentViewModel parent)
        {
            IsBusy = true;
            ProgressMessage = "Getting data..."; //TODO: ResX

            WorkerThread.Run(() =>
                {
                    var checking = "Checking: "; //TODO: ResX
                    var missing = new List<FileSystemItem>();
                    var html = new HtmlDocument();
                    html.LoadHtml(parent.HttpGetString("gettable.html?name=ContentItems", Encoding.UTF8));
                    foreach (var row in html.DocumentNode.QuerySelectorAll("table.GameContentHeader > tr").Skip(2))
                    {
                        var cells = row.SelectNodes("td")
                                       .Select((c, i) => new {key = (FsdContentItemProperty) i, value = c.InnerText.Trim()})
                                       .ToDictionary(k => k.key, k => k.value);
                        var contentId = Int32.Parse(cells[FsdContentItemProperty.Id]);
                        var title = cells[FsdContentItemProperty.Name];
                        UIThread.Run(() => { ProgressMessage = checking + title; });
                        var scanPathId = Int32.Parse(cells[FsdContentItemProperty.ScanPathId]);
                        var f = parent.ScanFolders[scanPathId];
                        var path = string.Format("/{0}{1}", f.Drive, cells[FsdContentItemProperty.Path].Replace("\\", "/"));
                        if (!parent.FileExists(path))
                        {
                            missing.Add(new FileSystemItem
                                            {
                                                Title = title,
                                                Path = path,
                                                Thumbnail = parent.HttpGet(string.Format("assets/gameicon.png?contentid={0:X2}", contentId))
                                            });
                        }
                    }
                    return missing;
                },
            result =>
                {
                    IsBusy = false;
                    ProgressMessage = string.Empty;
                    MissingEntries = result.Select(m => new FileSystemItemViewModel(m)).ToObservableCollection();
                },
            error =>
                {
                    IsBusy = false;
                    ProgressMessage = string.Empty;
                    //TODO
                });

        }

    }
}