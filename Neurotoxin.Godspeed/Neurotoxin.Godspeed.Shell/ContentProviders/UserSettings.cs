using Neurotoxin.Godspeed.Core.Caching;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class UserSettings
    {
        private static readonly EsentPersistentDictionary CacheStore = EsentPersistentDictionary.Instance;

        public const string DisableCustomChrome = "DisableCustomChrome";
        public const string UseRemoteCopy = "UseRemoteCopy";
        public const string LeftPaneType = "LeftPaneType";
        public const string LeftPaneFileListPaneSettings = "LeftPaneFileListPaneSettings";
        public const string RightPaneType = "RightPaneType";
        public const string RightPaneFileListPaneSettings = "RightPaneFileListPaneSettings";

        public static T Get<T>(string key, T defaultValue = default(T))
        {
            return CacheStore.ContainsKey(key) ? CacheStore.Get<T>(key) : defaultValue;
        }

        public static void Save<T>(string key, T value)
        {
            CacheStore.Update(key, value);
        }
    }
}