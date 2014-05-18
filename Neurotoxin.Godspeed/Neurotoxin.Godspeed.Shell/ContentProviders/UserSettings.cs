using System.ComponentModel;
using System.Globalization;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class UserSettings : IUserSettings
    {
        private readonly EsentPersistentDictionary CacheStore = EsentPersistentDictionary.Instance;

        private const string LanguageKey = "Language";
        public CultureInfo Language
        {
            get
            {
                var name = Get<string>(LanguageKey);
                return string.IsNullOrEmpty(name) ? null : CultureInfo.GetCultureInfo(name);
            }
            set { Set(LanguageKey, value != null ? value.Name: null); }
        }

        private const string DisableCustomChromeKey = "DisableCustomChrome";
        public bool DisableCustomChrome
        {
            get { return Get<bool>(DisableCustomChromeKey); }
            set { Set(DisableCustomChromeKey, value); }
        }

        private const string UseVersionCheckerKey = "UseVersionChecker";
        public bool UseVersionChecker
        {
            get { return Get(UseVersionCheckerKey, true); }
            set { Set(UseVersionCheckerKey, value); }
        }

        private const string VerifyFileHashAfterFtpUploadKey = "VerifyFileHashAfterFtpUpload";
        public bool VerifyFileHashAfterFtpUpload
        {
            get { return Get(VerifyFileHashAfterFtpUploadKey, false); }
            set { Set(VerifyFileHashAfterFtpUploadKey, value); }
        }

        private const string TriggerContentScanAfterUploadKey = "FsdContentScanTrigger";
        public FsdContentScanTrigger FsdContentScanTrigger
        {
            get { return Get(TriggerContentScanAfterUploadKey, FsdContentScanTrigger.AfterUpload); }
            set { Set(TriggerContentScanAfterUploadKey, value); }
        }

        private const string UseRemoteCopyKey = "UseRemoteCopy";
        public bool UseRemoteCopy
        {
            get { return Get<bool>(UseRemoteCopyKey); }
            set { Set(UseRemoteCopyKey, value); }
        }

        private const string UseJqe360Key = "UseJqe360";
		public bool UseJqe360
        {
            get { return Get(UseJqe360Key, true); }
            set { Set(UseJqe360Key, value); }
        }

        private const string ProfileExpirationKey = "ProfileExpiration";
		public int ProfileExpiration
        {
            get { return Get<int>(ProfileExpirationKey); }
            set { Set(ProfileExpirationKey, value); }
        }

        private const string ProfileInvalidationKey = "ProfileInvalidation";
		public bool ProfileInvalidation
        {
            get { return Get(ProfileInvalidationKey, true); }
            set { Set(ProfileInvalidationKey, value); }
        }

        private const string RecognizedGameExpirationKey = "RecognizedGameExpiration";
		public int RecognizedGameExpiration
        {
            get { return Get<int>(RecognizedGameExpirationKey); }
            set { Set(RecognizedGameExpirationKey, value); }
        }

        private const string PartiallyRecognizedGameExpirationKey = "PartiallyRecognizedGameExpiration";
		public int PartiallyRecognizedGameExpiration
        {
            get { return Get(PartiallyRecognizedGameExpirationKey, 7); }
            set { Set(PartiallyRecognizedGameExpirationKey, value); }
        }

        private const string UnrecognizedGameExpirationKey = "UnrecognizedGameExpiration";
		public int UnrecognizedGameExpiration
        {
            get { return Get(UnrecognizedGameExpirationKey, 7); }
            set { Set(UnrecognizedGameExpirationKey, value); }
        }

        private const string XboxLiveContentExpirationKey = "XboxLiveContentExpiration";
		public int XboxLiveContentExpiration
        {
            get { return Get(XboxLiveContentExpirationKey, 14); }
            set { Set(XboxLiveContentExpirationKey, value); }
        }

        private const string XboxLiveContentInvalidationKey = "XboxLiveContentInvalidation";
		public bool XboxLiveContentInvalidation
        {
            get { return Get(XboxLiveContentInvalidationKey, true); }
            set { Set(XboxLiveContentInvalidationKey, value); }
        }

        private const string UnknownContentExpirationKey = "UnknownContentExpiration";
		public int UnknownContentExpiration
        {
            get { return Get<int>(UnknownContentExpirationKey); }
            set { Set(UnknownContentExpirationKey, value); }
        }

        private const string LeftPaneTypeKey = "LeftPaneType";
		public string LeftPaneType
        {
            get { return Get(LeftPaneTypeKey, typeof(LocalFileSystemContentViewModel).FullName); }
            set { Set(LeftPaneTypeKey, value); }
        }

        private const string LeftPaneFileListPaneSettingsKey = "LeftPaneFileListPaneSettings";
		public FileListPaneSettings LeftPaneFileListPaneSettings
        {
            get { return Get(LeftPaneFileListPaneSettingsKey, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending)); }
            set { Set(LeftPaneFileListPaneSettingsKey, value); }
        }

        private const string RightPaneTypeKey = "RightPaneType";
		public string RightPaneType
        {
            get { return Get(RightPaneTypeKey, typeof(ConnectionsViewModel).FullName); }
            set { Set(RightPaneTypeKey, value); }
        }

        private const string RightPaneFileListPaneSettingsKey = "RightPaneFileListPaneSettings";
		public FileListPaneSettings RightPaneFileListPaneSettings
        {
            get { return Get(RightPaneFileListPaneSettingsKey, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending)); }
            set { Set(RightPaneFileListPaneSettingsKey, value); }
        }

        private const string DisableUserStatisticsParticipationKey = "DisableUserStatisticsParticipation";
        public bool? DisableUserStatisticsParticipation
        {
            get { return Get<bool?>(DisableUserStatisticsParticipationKey); }
            set { Set(DisableUserStatisticsParticipationKey, value); }
        }

        public bool IsMessageIgnored(string message)
        {
            return CacheStore.ContainsKey(Strings.UserMessageCacheItemPrefix + message.Hash());
        }

        public void IgnoreMessage(string message)
        {
            CacheStore.Update(Strings.UserMessageCacheItemPrefix + message.Hash(), true);
        }

        private T Get<T>(string key, T defaultValue = default(T))
        {
            return CacheStore.ContainsKey(key) ? CacheStore.Get<T>(key) : defaultValue;
        }

        private void Set<T>(string key, T value)
        {
            CacheStore.Update(key, value);
        }
    }
}