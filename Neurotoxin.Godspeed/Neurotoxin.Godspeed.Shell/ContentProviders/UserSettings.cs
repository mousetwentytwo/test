using System.ComponentModel;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class UserSettings
    {
        private static readonly EsentPersistentDictionary CacheStore = EsentPersistentDictionary.Instance;

        public const string DisableCustomChromeKey = "DisableCustomChrome";
        public static bool DisableCustomChrome
        {
            get { return Get<bool>(DisableCustomChromeKey); }
            set { Set(DisableCustomChromeKey, value); }
        }

        public const string UseRemoteCopyKey = "UseRemoteCopy";
        public static bool UseRemoteCopy
        {
            get { return Get<bool>(UseRemoteCopyKey); }
            set { Set(UseRemoteCopyKey, value); }
        }

        public const string UseJqe360Key = "UseJqe360";
		public static bool UseJqe360
        {
            get { return Get(UseJqe360Key, true); }
            set { Set(UseJqe360Key, value); }
        }

        public const string ProfileExpirationKey = "ProfileExpiration";
		public static int ProfileExpiration
        {
            get { return Get<int>(ProfileExpirationKey); }
            set { Set(ProfileExpirationKey, value); }
        }

        public const string ProfileInvalidationKey = "ProfileInvalidation";
		public static bool ProfileInvalidation
        {
            get { return Get(ProfileInvalidationKey, true); }
            set { Set(ProfileInvalidationKey, value); }
        }

        public const string RecognizedGameExpirationKey = "RecognizedGameExpiration";
		public static int RecognizedGameExpiration
        {
            get { return Get<int>(RecognizedGameExpirationKey); }
            set { Set(RecognizedGameExpirationKey, value); }
        }

        public const string PartiallyRecognizedGameExpirationKey = "PartiallyRecognizedGameExpiration";
		public static int PartiallyRecognizedGameExpiration
        {
            get { return Get<int>(PartiallyRecognizedGameExpirationKey, 7); }
            set { Set(PartiallyRecognizedGameExpirationKey, value); }
        }

        public const string UnrecognizedGameExpirationKey = "UnrecognizedGameExpiration";
		public static int UnrecognizedGameExpiration
        {
            get { return Get<int>(UnrecognizedGameExpirationKey, 7); }
            set { Set(UnrecognizedGameExpirationKey, value); }
        }

        public const string XboxLiveContentExpirationKey = "XboxLiveContentExpiration";
		public static int XboxLiveContentExpiration
        {
            get { return Get<int>(XboxLiveContentExpirationKey, 14); }
            set { Set(XboxLiveContentExpirationKey, value); }
        }

        public const string XboxLiveContentInvalidationKey = "XboxLiveContentInvalidation";
		public static bool XboxLiveContentInvalidation
        {
            get { return Get(XboxLiveContentInvalidationKey, true); }
            set { Set(XboxLiveContentInvalidationKey, value); }
        }

        public const string UnknownContentExpirationKey = "UnknownContentExpiration";
		public static int UnknownContentExpiration
        {
            get { return Get<int>(UnknownContentExpirationKey); }
            set { Set(UnknownContentExpirationKey, value); }
        }

        public const string LeftPaneTypeKey = "LeftPaneType";
		public static string LeftPaneType
        {
            get { return Get(LeftPaneTypeKey, typeof(LocalFileSystemContentViewModel).FullName); }
            set { Set(LeftPaneTypeKey, value); }
        }

        public const string LeftPaneFileListPaneSettingsKey = "LeftPaneFileListPaneSettings";
		public static FileListPaneSettings LeftPaneFileListPaneSettings
        {
            get { return Get(LeftPaneFileListPaneSettingsKey, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending)); }
            set { Set(LeftPaneFileListPaneSettingsKey, value); }
        }

        public const string RightPaneTypeKey = "RightPaneType";
		public static string RightPaneType
        {
            get { return Get(RightPaneTypeKey, typeof(ConnectionsViewModel).FullName); }
            set { Set(RightPaneTypeKey, value); }
        }

        public const string RightPaneFileListPaneSettingsKey = "RightPaneFileListPaneSettings";
		public static FileListPaneSettings RightPaneFileListPaneSettings
        {
            get { return Get(RightPaneFileListPaneSettingsKey, new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending)); }
            set { Set(RightPaneFileListPaneSettingsKey, value); }
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