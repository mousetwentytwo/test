using System;
using System.ComponentModel;
using System.Reflection;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class UserSettings
    {
        private readonly EsentPersistentDictionary _cacheStore = EsentPersistentDictionary.Instance;

        public const string LeftPane = "LeftPane";
        public const string RightPane = "RightPane";

        private static UserSettings _instance;
        public static UserSettings Instance
        {
            get { return _instance ?? (_instance = new UserSettings()); }
        }

        private UserSettings()
        {
            LeftPaneType = GetPaneType(LEFTPANETYPE, typeof(LocalFileSystemContentViewModel));
            RightPaneType = GetPaneType(RIGHTPANETYPE, typeof(ConnectionsViewModel));
            LeftPaneFileListPaneSettings = _cacheStore.ContainsKey(LEFTPANEFILELISTPANESETTINGS)
                                               ? _cacheStore.Get<FileListPaneSettings>(LEFTPANEFILELISTPANESETTINGS)
                                               : new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending);
            RightPaneFileListPaneSettings = _cacheStore.ContainsKey(RIGHTPANEFILELISTPANESETTINGS)
                                               ? _cacheStore.Get<FileListPaneSettings>(RIGHTPANEFILELISTPANESETTINGS)
                                               : new FileListPaneSettings(@"C:\", "ComputedName", ListSortDirection.Ascending);
        }

        private Type GetPaneType(string key, Type defaultValue)
        {
            var asm = Assembly.GetExecutingAssembly();
            var storedValue = _cacheStore.ContainsKey(key) ? asm.GetType(_cacheStore.Get<string>(key)) : null;
            return storedValue ?? defaultValue;
        }

        private const string LEFTPANETYPE = "LeftPaneType";
        public Type LeftPaneType { get; private set; }

        private const string LEFTPANEFILELISTPANESETTINGS = "LeftPaneFileListPaneSettings";
        public FileListPaneSettings LeftPaneFileListPaneSettings { get; private set; }

        private const string RIGHTPANETYPE = "RightPaneType";
        public Type RightPaneType { get; private set; }

        private const string RIGHTPANEFILELISTPANESETTINGS = "RightPaneFileListPaneSettings";
        public FileListPaneSettings RightPaneFileListPaneSettings { get; private set; }

        public void SavePaneSettings(Type type, FileListPaneSettings settings, string keyPrefix)
        {
            string typeKey;
            string settingsKey;
            switch (keyPrefix)
            {
                case LeftPane:
                    typeKey = LEFTPANETYPE;
                    settingsKey = LEFTPANEFILELISTPANESETTINGS;
                    break;
                case RightPane:
                    typeKey = RIGHTPANETYPE;
                    settingsKey = RIGHTPANEFILELISTPANESETTINGS;
                    break;
                default:
                    throw new NotSupportedException("Invalid key prefix: " + keyPrefix);
            }

            _cacheStore.Update(typeKey, type.FullName);
            _cacheStore.Update(settingsKey, settings);
        }
    }
}