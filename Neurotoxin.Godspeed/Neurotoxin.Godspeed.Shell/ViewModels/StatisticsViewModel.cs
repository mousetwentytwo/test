using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        private const string PARTICIPATIONMESSAGE = "<b>Help improve GODspeed!</b> Click to set your participation in its User Statistics.";
        private const string FACEBOOKMESSAGE = "<b>Do you like GODspeed?</b> Do you want to get news about it first hand, share your ideas and/or be a part of its growing community? Then please like its Facebook page!";
        private const string CODEPLEXMESSAGE = "<b>Thank you for using GODspeed!</b> Do you think you've got an opinion about the current version? Rate and review it on CodePlex! Thanks!";

        private readonly Timer _participationTimer;
        private readonly Timer _facebookTimer;
        private readonly Timer _codeplexTimer;
        private readonly CacheManager _cacheManager;
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;
        private readonly IEventAggregator _eventAggregator;

        public DateTime UsageStart { get; private set; }
        public Dictionary<string, int> CommandUsage { get; private set; }

        private const string GAMESRECOGNIZEDFULLY = "GamesRecognizedFully";
        public int GamesRecognizedFully
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == TitleType.Game && c.RecognitionState == RecognitionState.Recognized); }
        }

        private const string GAMESRECOGNIZEDPARTIALLY = "GamesRecognizedPartially";
        public int GamesRecognizedPartially
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == TitleType.Game && c.RecognitionState == RecognitionState.PartiallyRecognized); }
        }

        private const string SVODPACKAGESRECOGNIZED = "SvodPackagesRecognized";
        public int SvodPackagesRecognized
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == TitleType.Unknown && c.ContentType != ContentType.Unknown); }
        }

        private const string STFSPACKAGESRECOGNIZED = "StfsPackagesRecognized";
        public int StfsPackagesRecognized
        {
            get { return _cacheManager.EntryCount(c => c.TitleType == TitleType.Profile); }
        }

        private const string FILESTRANSFERRED = "FilesTransferred";
        private int _filesTransferred;
        public int FilesTransferred
        {
            get { return _filesTransferred; }
            set { _filesTransferred = value; NotifyPropertyChanged(FILESTRANSFERRED); }
        }

        private const string STAT_TOTALFILESTRANSFERRED = "Stat_TotalFilesTransferred";
        private const string TOTALFILESTRANSFERRED = "TotalFilesTransferred";
        private int _totalFilesTransferred;
        public int TotalFilesTransferred
        {
            get { return _totalFilesTransferred + FilesTransferred; }
            set { _totalFilesTransferred = value; NotifyPropertyChanged(TOTALFILESTRANSFERRED); }
        }

        private const string BYTESTRANSFERRED = "BytesTransferred";
        private long _bytesTransferred;
        public long BytesTransferred
        {
            get { return _bytesTransferred; }
            set { _bytesTransferred = value; NotifyPropertyChanged(BYTESTRANSFERRED); }
        }

        private const string STAT_TOTALBYTESTRANSFERRED = "Stat_TotalBytesTransferred";
        private const string TOTALBYTESTRANSFERRED = "TotalBytesTransferred";
        private long _totalBytesTransferred;
        public long TotalBytesTransferred
        {
            get { return _totalBytesTransferred + BytesTransferred; }
            set { _totalBytesTransferred = value; NotifyPropertyChanged(TOTALBYTESTRANSFERRED); }
        }

        private const string TIMESPENTWITHTRANSFER = "TimeSpentWithTransfer";
        private TimeSpan _timeSpentWithTransfer;
        public TimeSpan TimeSpentWithTransfer
        {
            get { return _timeSpentWithTransfer; }
            set { _timeSpentWithTransfer = value; NotifyPropertyChanged(TIMESPENTWITHTRANSFER); }
        }

        private const string STAT_TOTALTIMESPENTWITHTRANSFER = "Stat_TotalTimeSpentWithTransfer";
        private const string TOTALTIMESPENTWITHTRANSFER = "TotalTimeSpentWithTransfer";
        private TimeSpan _totalTimeSpentWithTransfer;
        public TimeSpan TotalTimeSpentWithTransfer
        {
            get { return _totalTimeSpentWithTransfer + TimeSpentWithTransfer; }
            set { _totalTimeSpentWithTransfer = value; NotifyPropertyChanged(TOTALTIMESPENTWITHTRANSFER); }
        }

        private const string USAGETIME = "UsageTime";
        public TimeSpan UsageTime
        {
            get { return DateTime.Now.Subtract(UsageStart); }
        }

        private const string STAT_TOTALUSAGETIME = "Stat_TotalUsageTime";
        private const string TOTALUSAGETIME = "TotalUsageTime";
        private TimeSpan _totalUsageTime;
        public TimeSpan TotalUsageTime
        {
            get { return _totalUsageTime + UsageTime; }
            set { _totalUsageTime = value; NotifyPropertyChanged(TOTALUSAGETIME); }
        }

        private const string STAT_APPLICATIONSTARTED = "Stat_ApplicationStarted";
        private const string APPLICATIONSTARTED = "ApplicationStarted";
        private int _applicationStarted;
        public int ApplicationStarted
        {
            get { return _applicationStarted; }
            set { _applicationStarted = value; NotifyPropertyChanged(APPLICATIONSTARTED); }
        }

        private const string STAT_APPLICATIONCRASHED = "Stat_ApplicationCrashed";
        private const string APPLICATIONCRASHED = "ApplicationCrashed";
        private int _applicationCrashed;
        public int ApplicationCrashed
        {
            get { return _applicationCrashed; }
            set { _applicationCrashed = value; NotifyPropertyChanged(APPLICATIONCRASHED); }
        }

        public StatisticsViewModel(CacheManager cacheManager, IEventAggregator eventAggregator)
        {
            UsageStart = DateTime.Now;
            CommandUsage = new Dictionary<string, int>();
            _cacheManager = cacheManager;
            _eventAggregator = eventAggregator;
            _totalFilesTransferred = _cacheStore.TryGet<int>(STAT_TOTALFILESTRANSFERRED);
            _totalBytesTransferred = _cacheStore.TryGet<long>(STAT_TOTALBYTESTRANSFERRED);
            _totalTimeSpentWithTransfer = _cacheStore.TryGet<TimeSpan>(STAT_TOTALTIMESPENTWITHTRANSFER);
            _totalUsageTime = _cacheStore.TryGet<TimeSpan>(STAT_TOTALUSAGETIME);
            _applicationStarted = _cacheStore.TryGet<int>(STAT_APPLICATIONSTARTED) + 1;
            _applicationCrashed = _cacheStore.TryGet<int>(STAT_APPLICATIONCRASHED);

            //TODO: event?
            DelegateCommand.BeforeAction = CountCommandUsage;

            //TODO: not sure this is the right place to implement
            if (!UserSettings.DisableUserStatisticsParticipation.HasValue)
            {
                _participationTimer = new Timer(ParticipationMessage, null, 60000, -1);
            }
            if (!UserSettings.IsMessageIgnored(FACEBOOKMESSAGE))
            {
                _facebookTimer = new Timer(FacebookMessage, null, 600000, -1);
            }
            if (!UserSettings.IsMessageIgnored(CODEPLEXMESSAGE) && ApplicationStarted > 9 && TotalUsageTime > new TimeSpan(0, 2, 0, 0))
            {
                _codeplexTimer = new Timer(CodeplexMessage, null, 60000, -1);
            }
        }

        public void PersistData()
        {
            _cacheStore.Update(STAT_TOTALFILESTRANSFERRED, TotalFilesTransferred);
            _cacheStore.Update(STAT_TOTALBYTESTRANSFERRED, TotalBytesTransferred);
            _cacheStore.Update(STAT_TOTALTIMESPENTWITHTRANSFER, TotalTimeSpentWithTransfer);
            _cacheStore.Update(STAT_TOTALUSAGETIME, TotalUsageTime);
            _cacheStore.Update(STAT_APPLICATIONSTARTED, ApplicationStarted);
            _cacheStore.Update(STAT_APPLICATIONCRASHED, ApplicationCrashed);
        }

        private void CountCommandUsage(MethodInfo commandAction)
        {
            var declaringType = commandAction.DeclaringType;
            var className = string.Empty;
            if (declaringType != null)
            {
                className = declaringType.Name;
                var genericArgs = declaringType.GetGenericArguments();
                for (var i = 0; i < genericArgs.Length; i++)
                {
                    var genericType = genericArgs[i];
                    var prefix = i == 0 ? "<" : ",";
                    var suffix = i == genericArgs.Length - 1 ? ">" : string.Empty;
                    className = className.Replace(string.Format("`{0}", i + 1), string.Format("{0}{1}{2}", prefix, genericType.Name, suffix));
                }
                className += ".";
            }
            var command = className + commandAction.Name;
            if (CommandUsage.ContainsKey(command))
                CommandUsage[command]++;
            else
                CommandUsage.Add(command, 1);
        }

        private void ParticipationMessage(object state)
        {
            _participationTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(PARTICIPATIONMESSAGE, MessageIcon.Info, MessageCommand.OpenDialog, typeof(UserStatisticsParticipationDialog));
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

        private void FacebookMessage(object state)
        {
            _facebookTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(FACEBOOKMESSAGE, MessageIcon.Info, MessageCommand.OpenUrl, "http://www.facebook.com/godspeedftp", MessageFlags.Ignorable | MessageFlags.IgnoreAfterOpen);
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }

        private void CodeplexMessage(object state)
        {
            _codeplexTimer.Dispose();
            var args = new NotifyUserMessageEventArgs(CODEPLEXMESSAGE, MessageIcon.Info, MessageCommand.OpenUrl, "http://godspeed.codeplex.com", MessageFlags.Ignorable | MessageFlags.IgnoreAfterOpen);
            _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(args);
        }
    }
}