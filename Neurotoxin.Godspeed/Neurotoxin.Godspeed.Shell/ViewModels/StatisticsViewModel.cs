using System;
using System.Collections.Generic;
using System.Reflection;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StatisticsViewModel : ViewModelBase, IStatisticsViewModel
    {
        private readonly CacheManager _cacheManager;
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

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

        public StatisticsViewModel(CacheManager cacheManager)
        {
            UsageStart = DateTime.Now;
            CommandUsage = new Dictionary<string, int>();

            _cacheManager = cacheManager;
            _totalFilesTransferred = _cacheStore.TryGet<int>(STAT_TOTALFILESTRANSFERRED);
            _totalBytesTransferred = _cacheStore.TryGet<long>(STAT_TOTALBYTESTRANSFERRED);
            _totalTimeSpentWithTransfer = _cacheStore.TryGet<TimeSpan>(STAT_TOTALTIMESPENTWITHTRANSFER);
            _totalUsageTime = _cacheStore.TryGet<TimeSpan>(STAT_TOTALUSAGETIME);
            _applicationStarted = _cacheStore.TryGet<int>(STAT_APPLICATIONSTARTED) + 1;
            _applicationCrashed = _cacheStore.TryGet<int>(STAT_APPLICATIONCRASHED);

            DelegateCommand.BeforeAction = CountCommandUsage;
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
    }
}