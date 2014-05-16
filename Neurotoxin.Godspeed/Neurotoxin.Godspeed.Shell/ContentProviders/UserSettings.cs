using System.ComponentModel;
using System.Globalization;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Core.Extensions;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public static class UserSettings
    {
        private static readonly EsentPersistentDictionary CacheStore = EsentPersistentDictionary.Instance;

        private const string LanguageKey = "Language";
        public static CultureInfo Language
        {
            get
            {
                var name = Get<string>(LanguageKey);
                return string.IsNullOrEmpty(name) ? null : CultureInfo.GetCultureInfo(name);
            }
            set { Set(LanguageKey, value != null ? value.Name: null); }
        }

        private const string DisableCustomChromeKey = "DisableCustomChrome";
        public static bool DisableCustomChrome
        {
            get { return Get<bool>(DisableCustomChromeKey); }
            set { Set(DisableCustomChromeKey, value); }
        }

        private const string UseVersionCheckerKey = "UseVersionChecker";
        public static bool UseVersionChecker
        {
            get { return Get(UseVersionCheckerKey, true); }
            set { Set(UseVersionCheckerKey, value); }
        }

        private const string VerifyFileHashAfterFtpUploadKey = "VerifyFileHashAfterFtpUpload";
        public static bool VerifyFileHashAfterFtpUpload
        {
            get { return Get(VerifyFileHashAfterFtpUploadKey, false); }
            set { Set(VerifyFileHashAfterFtpUploadKey, value); }
        }

        private const string TriggerContentScanAfterUploadKey = "FsdContentScanTrigger";
        public static FsdContentScanTrigger FsdContentScanTrigger
        {
            get { return Get(TriggerContentScanAfterUploadKey, FsdContentScanTrigger.AfterUpload); }
            set { Set(TriggerContentScanAfterUploadKey, value); }
        }

        private const string UseRemoteCopyKey = "UseRemoteCopy";
        public static bool UseRemoteCopy
        {
            get { return Get<bool>(UseRemoteCopyKey); }
            set { Set(UseRemoteCopyKey, value); }
        }

        private const string UseJqe360Key = "UseJqe360";
		public static bool UseJqe360
        {
            get { return Get(UseJqe360Key, true); }
            set { Set(UseJqe360Key, value); }
        }

        private const string ProfileExpirationKey = "ProfileExpiration";
		public static int ProfileExpiration
        {
            get { return Get<int>(ProfileExpirationKey); }
            set { Set(ProfileExpirationKey, value); }
        }

        private const string ProfileInvalidationKey = "ProfileInvalidation";
		public static bool ProfileInvalidation
        {
            get { return Get(ProfileInvalidationKey, true); }
            set { Set(ProfileInvalidationKey, value); }
        }

        private const string RecognizedGameExpirationKey = "RecognizedGameExpiration";
		public static int RecognizedGameExpiration
        {
            get { return Get<int>(RecognizedGameExpirationKey); }
            set { Set(RecognizedGameExpirationKey, value); }
        }

        private const string PartiallyRecognizedGameExpirationKey = "PartiallyRecognizedGameExpiration";
		public static int PartiallyRecognizedGameExpiration
        {
            get { return Get(PartiallyRecognizedGameExpirationKey, 7); }
            set { Set(PartiallyRecognizedGameExpirationKey, value); }
        }

        private const string UnrecognizedGameExpirationKey = "UnrecognizedGameExpiration";
		public static int UnrecognizedGameExpiration
        {
            get { return Get(UnrecognizedGameExpirationKey, 7); }
            set { Set(UnrecognizedGameExpirationKey, value); }
        }

        private const string XboxLiveContentExpirationKey = "XboxLiveContentExpiration";
		public static int XboxLiveContentExpiration
        {
            get { return Get(XboxLiveContentExpirationKey, 14); }
            set { Set(XboxLiveContentExpirationKey, value); }
        }

        private const string XboxLiveContentInvalidationKey = "XboxLiveContentInvalidation";
		public static bool XboxLiveContentInvalidation
        {
            get { return Get(XboxLiveContentInvalidationKey, true); }
            set { Set(XboxLiveContentInvalidationKey, value); }
        }

        private const string UnknownContentExpirationKey = "UnknownContentExpiration";
		public static int UnknownContentExpiration
        {
            get { return Get<int>(UnknownContentExpirationKey); }
            set { Set(UnknownContentExpirationKey, value); }
        }

        private const string LeftPaneTypeKey = "LeftPaneType";
		public static string LeftPaneType
        {
            get { return Get(LeftPaneTypeKey, typeof(LocalFileSystemContentViewModel).FullName); }
            set { Set(LeftPaneTypeKey, value); }
        }

        private const string LeftPaneFileListPaneSettingsKey = "LeftPaneFileListPaneSettings";
		public static FileListPaneSettings LeftPaneFileListPaneSettings
        {
            get { return Get(LeftPaneFileListPaneSettingsKey, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending)); }
            set { Set(LeftPaneFileListPaneSettingsKey, value); }
        }

        private const string RightPaneTypeKey = "RightPaneType";
		public static string RightPaneType
        {
            get { return Get(RightPaneTypeKey, typeof(ConnectionsViewModel).FullName); }
            set { Set(RightPaneTypeKey, value); }
        }

        private const string RightPaneFileListPaneSettingsKey = "RightPaneFileListPaneSettings";
		public static FileListPaneSettings RightPaneFileListPaneSettings
        {
            get { return Get(RightPaneFileListPaneSettingsKey, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending)); }
            set { Set(RightPaneFileListPaneSettingsKey, value); }
        }

        private const string DisableUserStatisticsParticipationKey = "DisableUserStatisticsParticipation";
        public static bool? DisableUserStatisticsParticipation
        {
            get { return Get<bool?>(DisableUserStatisticsParticipationKey); }
            set { Set(DisableUserStatisticsParticipationKey, value); }
        }

        public static bool IsMessageIgnored(string message)
        {
            return CacheStore.ContainsKey(Strings.UserMessageCacheItemPrefix + message.Hash());
        }

        public static void IgnoreMessage(string message)
        {
            CacheStore.Update(Strings.UserMessageCacheItemPrefix + message.Hash(), true);
        }

        private static T Get<T>(string key, T defaultValue = default(T))
        {
            return CacheStore.ContainsKey(key) ? CacheStore.Get<T>(key) : defaultValue;
        }

        private static void Set<T>(string key, T value)
        {
            CacheStore.Update(key, value);
        }
    }
}