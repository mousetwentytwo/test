using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class TitleRecognizer
    {
        private readonly IFileManager _fileManager;
        private readonly CacheManager _cacheManager;
        private readonly Dictionary<string, FileSystemItem> _profileFileCache = new Dictionary<string, FileSystemItem>();

        private static readonly List<RecognitionInformation> RecognitionKeywords = new List<RecognitionInformation>
            {
                new RecognitionInformation("^0000000000000000$", "Games", TitleType.SystemDir),
                new RecognitionInformation("^584E07D2$", "XNA Indie Player", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07C3$", "Gamer Pictures", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07D1$", "Profile Data", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07DF$", "Avatar Editor", TitleType.SystemDir),
                new RecognitionInformation("^F[0-9A-F]{7}$", "System Data", TitleType.SystemDir),
                new RecognitionInformation("^F[0-9A-F]{7}.gpd$", "System Data", TitleType.SystemFile, ItemType.File),
                new RecognitionInformation("^[1-9A-E][0-9A-F]{7}$", "Unknown Game", TitleType.Game),
                new RecognitionInformation("^[1-9A-E][0-9A-F]{7}.gpd$", "Unknown Game", TitleType.Game, ItemType.File),
                new RecognitionInformation("^[0-9A-F]{8}$", "Unknown Content", TitleType.Content),
                new RecognitionInformation("^E0000[0-9A-F]{11}$", "Unknown Profile", TitleType.Profile, ItemType.Directory | ItemType.File),
            };

        public TitleRecognizer(IFileManager fileManager, CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _fileManager = fileManager;
        }

        public static bool IsXboxFolder(FileSystemItem item)
        {
            if (string.IsNullOrEmpty(item.Name)) return false;
            if (item.Type != ItemType.Directory) return false;
            return RecognizeByName(item.Name) != null;
        }

        public static bool RecognizeType(FileSystemItem item)
        {
            var recognition = RecognizeByName(item.Name, item.Type);
            if (recognition == null) return false;

            item.TitleType = recognition.TitleType;
            if (recognition.TitleType == TitleType.Content)
            {
                var content = BitConverter.ToInt32(item.Name.FromHex(), 0);
                if (Enum.IsDefined(typeof (ContentType), content))
                {
                    item.ContentType = (ContentType) content;
                    item.Title = GetContentTypeTitle(item.ContentType);
                }
            }
            else
            {
                item.Title = recognition.Title;
            }
            return true;
        }

        public static string GetTitle(string name)
        {
            var recognition = RecognizeByName(name);
            if (recognition == null) return null;
            if (recognition.TitleType == TitleType.Content)
            {
                var content = BitConverter.ToInt32(name.FromHex(), 0);
                if (Enum.IsDefined(typeof (ContentType), content))
                    return GetContentTypeTitle((ContentType) content);
            }
            return recognition.Title;
        }

        private static string GetContentTypeTitle(ContentType contentType)
        {
            var title = EnumHelper.GetStringValue(contentType);
            var suffix = title.EndsWith("data", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : "s";
            return string.Format("{0}{1}", title, suffix);
        }

        private static RecognitionInformation RecognizeByName(string name, ItemType? flag = null)
        {
            var recognition = RecognitionKeywords.FirstOrDefault(r => new Regex(r.Pattern, RegexOptions.IgnoreCase).IsMatch(name));
            return recognition != null && flag.HasValue && !recognition.ItemTypeFlags.HasFlag(flag.Value) ? null : recognition;
        }

        public bool MergeWithCachedEntry(FileSystemItem item, FileSystemItem cacheItem = null)
        {
            if (cacheItem == null) cacheItem = GetCacheItem(item);
            var cacheKey = GetCacheKey(cacheItem);

            var cachedItem = _cacheManager.GetEntry(cacheKey, cacheItem.Size, cacheItem.Date);
            if (cachedItem != null)
            {
                if (cachedItem.Content != null)
                {
                    item.Title = cachedItem.Content.Title;
                    item.TitleType = cachedItem.Content.TitleType;
                    item.ContentType = cachedItem.Content.ContentType;
                    item.Thumbnail = cachedItem.Content.Thumbnail;
                }
                item.IsCached = true;
                return true;
            }
            return false;
        }

        public void RecognizeTitle(FileSystemItem item)
        {
            if (item.TitleType == TitleType.SystemDir || item.TitleType == TitleType.SystemFile) return;
            var cacheItem = GetCacheItem(item);
            if (MergeWithCachedEntry(item, cacheItem)) return;

            var cacheKey = GetCacheKey(cacheItem);

            switch (item.TitleType)
            {
                case TitleType.Profile:
                    if (cacheItem.Type == ItemType.File) GetProfileData(item, cacheItem);
                    var profileExpiration = GetExpirationFrom(UserSettings.ProfileExpiration);
                    if (UserSettings.ProfileInvalidation)
                    {
                        _cacheManager.SaveEntry(cacheKey, item, profileExpiration, cacheItem.Date, cacheItem.Size, _fileManager.TempFilePath);
                    }
                    else
                    {
                        _cacheManager.SaveEntry(cacheKey, item, profileExpiration);
                        File.Delete(_fileManager.TempFilePath);
                    }
                    item.IsCached = true;
                    break;
                case TitleType.Game:
                    if (item.Type == ItemType.File)
                    {
                        GetGameDataFromGpd(item);
                        _cacheManager.SaveEntry(cacheKey, item, GetExpirationFrom(UserSettings.RecognizedGameExpiration));
                    } 
                    else
                    {
                        var gameExpiration = GetGameData(item)
                                                 ? UserSettings.RecognizedGameExpiration
                                                 : UserSettings.UseJqe360 && GetGameDataFromJqe360(item)
                                                       ? UserSettings.PartiallyRecognizedGameExpiration
                                                       : UserSettings.UnrecognizedGameExpiration;
                        _cacheManager.SaveEntry(cacheKey, item, GetExpirationFrom(gameExpiration));
                    }
                    item.IsCached = true;
                    break;
                case TitleType.Content:
                    _cacheManager.SaveEntry(cacheKey, item);
                    item.IsCached = true;
                    break;
                case TitleType.Unknown:
                    if (item.Type == ItemType.File)
                    {
                        var header = _fileManager.ReadFileHeader(item.Path);
                        var svod = ModelFactory.GetModel<SvodPackage>(header);
                        if (svod.IsValid)
                        {
                            item.Title = svod.DisplayName;
                            item.Thumbnail = svod.ThumbnailImage;
                            item.ContentType = svod.ContentType;
                            var svodExpiration = GetExpirationFrom(UserSettings.XboxLiveContentExpiration);
                            if (UserSettings.XboxLiveContentInvalidation)
                            {
                                _cacheManager.SaveEntry(cacheKey, item, svodExpiration, item.Date, item.Size);
                            }
                            else
                            {
                                _cacheManager.SaveEntry(cacheKey, item, svodExpiration);
                            }
                        }
                        else
                        {
                            _cacheManager.SaveEntry(cacheKey, null, GetExpirationFrom(UserSettings.UnknownContentExpiration));
                        }
                        item.IsCached = true;
                    }
                    break;
            }
        }

        private static DateTime? GetExpirationFrom(int expiration)
        {
            if (expiration == 0) return null;
            return DateTime.Now.AddDays(expiration);
        }

        public FileSystemItem GetProfileItem(FileSystemItem item)
        {
            var profilePath = string.Format("{1}FFFE07D1{2}00010000{2}{0}", item.Name, item.Path, _fileManager.Slash);
            if (_profileFileCache.ContainsKey(profilePath)) return _profileFileCache[profilePath];
            if (_fileManager.FileExists(profilePath))
            {
                var profileItem = _fileManager.GetFileInfo(profilePath);
                if (profileItem == null)
                {
                    NotificationMessage.ShowMessage("Title recognition", string.Format("The profile with the ID {0} is currently in use. Please sign out.", item.Name));
                    return null;
                }
                RecognizeType(profileItem);
                _profileFileCache.Add(profilePath, profileItem);
                return profileItem;
            }
            return null;
        }

        private FileSystemItem GetCacheItem(FileSystemItem item)
        {
            return item.TitleType == TitleType.Profile && item.Type == ItemType.Directory
                       ? (GetProfileItem(item) ?? item)
                       : item;
        }

        private string GetCacheKey(FileSystemItem item)
        {
            switch (item.TitleType)
            {
                case TitleType.SystemDir:
                case TitleType.SystemFile:
                case TitleType.Content:
                    return item.Name;
                case TitleType.Game:
                    return item.Type == ItemType.File ? item.Name.Replace(".gpd", string.Empty) : item.Name;
                default:
                    return item.FullPath;
            }
        }

        private bool GetProfileData(FileSystemItem item, FileSystemItem cacheItem)
        {
            try
            {
                var bytes = _fileManager.ReadFileContent(cacheItem.Path, true, cacheItem.Size ?? 0);
                var stfs = ModelFactory.GetModel<StfsPackage>(bytes);
                stfs.ExtractAccount();
                item.Title = stfs.Account.GamerTag;
                item.Thumbnail = stfs.ThumbnailImage;
                item.ContentType = stfs.ContentType;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GetGameData(FileSystemItem item)
        {
            var infoFileFound = false;

            var systemdir = item.Name.StartsWith("5841") ? "000D0000" : "00007000";

            var gamePath = string.Format("{0}{1}", item.Path, systemdir);
            var exists = _fileManager.FolderExists(gamePath);
            if (!exists)
            {
                var r = new Regex(@"(?<content>Content[\\/])E0000[0-9A-F]{11}", RegexOptions.IgnoreCase);
                if (r.IsMatch(gamePath))
                {
                    gamePath = r.Replace(gamePath, "${content}0000000000000000");
                    exists = _fileManager.FolderExists(gamePath);
                }
            }

            if (exists)
            {
                var file = _fileManager.GetList(gamePath).FirstOrDefault(i => i.Type == ItemType.File);
                if (file != null)
                {
                    var fileContent = _fileManager.ReadFileHeader(file.Path);
                    var svod = ModelFactory.GetModel<SvodPackage>(fileContent);
                    if (svod.IsValid)
                    {
                        item.Title = svod.TitleName;
                        item.Thumbnail = svod.ThumbnailImage;
                        item.ContentType = svod.ContentType;
                        infoFileFound = true;
                    }
                }
            }
            return infoFileFound;
        }

        private static bool GetGameDataFromJqe360(FileSystemItem item)
        {
            var request = WebRequest.Create(string.Format("http://covers.jqe360.com/main.php?search={0}", item.Name));
            string title = null;
            var result = false;
            try
            {
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    var responseReader = new StreamReader(stream);
                    var htmlText = responseReader.ReadToEnd();
                    var regex = new Regex(string.Format("Title: .*?>(.*?)<.*?TitleID: {0}", item.Name), RegexOptions.IgnoreCase);
                    title = regex.Match(htmlText).Groups[1].Value;
                    result = !string.IsNullOrEmpty(title.Trim());
                }
            }
            catch
            {
                //TODO: ?
            }
            if (result) item.Title = title;
            item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
            return result;
        }

        private void GetGameDataFromGpd(FileSystemItem item)
        {
            var fileContent = _fileManager.ReadFileContent(item.Path, false, item.Size ?? 0);
            var gpd = ModelFactory.GetModel<GameFile>(fileContent);
            gpd.Parse();
            if (gpd.Strings.Count > 0) item.Title = gpd.Strings.First().Text;
            item.Thumbnail = gpd.Thumbnail;
        }

        public void UpdateCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            _cacheManager.UpdateEntry(cacheKey, item);
        }

        public void ThrowCache(FileSystemItem item)
        {
            var cacheItem = GetCacheItem(item);
            var cacheKey = GetCacheKey(cacheItem);
            _cacheManager.ClearCache(cacheKey);
        }

        public string GetTempFilePath(FileSystemItem item)
        {
            var cacheItem = GetCacheItem(item);
            var cacheKey = GetCacheKey(cacheItem);
            var cacheEntry = _cacheManager.GetEntry(cacheKey, cacheItem.Size, cacheItem.Date);
            return cacheEntry != null && !string.IsNullOrEmpty(cacheEntry.TempFilePath) ? cacheEntry.TempFilePath : null;
        }

        public CacheEntry<FileSystemItem> GetCacheEntry(string key)
        {
            return _cacheManager.GetEntry(key);
        }
    }
}