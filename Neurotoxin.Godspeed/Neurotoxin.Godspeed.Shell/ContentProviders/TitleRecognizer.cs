using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Gpd;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class TitleRecognizer
    {
        private readonly IFileManager _fileManager;
        private readonly CacheManager _cacheManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly Dictionary<string, FileSystemItem> _profileFileCache = new Dictionary<string, FileSystemItem>();

        private const string ACCESSERRORMESSAGE = "Inaccessible file. Please check the corresponding permissions.";

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
                new RecognitionInformation("^TU[\\w\\.]+$|^[0-9A-F]+$", "Unknown Data File", TitleType.Unknown, ItemType.File),
            };

        public TitleRecognizer(IFileManager fileManager, CacheManager cacheManager, IEventAggregator eventAggregator)
        {
            _cacheManager = cacheManager;
            _fileManager = fileManager;
            _eventAggregator = eventAggregator;
        }

        public static bool IsXboxItem(FileSystemItem item)
        {
            return RecognizeByName(item.Name, item.Type) != null;
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

        public bool MergeWithCachedEntry(FileSystemItem item)
        {
            if (item.TitleType == TitleType.SystemDir || item.TitleType == TitleType.SystemFile) return true;

            var cacheKey = GetCacheKey(item);
            var storedItem = _cacheManager.GetEntry(cacheKey.Key);
            if (storedItem != null)
            {
                if (storedItem.Content != null)
                {
                    item.Title = storedItem.Content.Title;
                    item.TitleType = storedItem.Content.TitleType;
                    item.ContentType = storedItem.Content.ContentType;
                    item.Thumbnail = storedItem.Content.Thumbnail;
                }
            }

            if (cacheKey.Item == null)
            {
                item.IsCached = true;
                item.IsLocked = true;
                item.LockMessage = cacheKey.ErrorMessage ?? ACCESSERRORMESSAGE;
            } 
            else
            {
                var validCacheItem = _cacheManager.GetEntry(cacheKey);
                item.IsCached = validCacheItem != null;
            }

            return item.IsCached;
        }

        public void RecognizeTitle(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);

            switch (item.TitleType)
            {
                case TitleType.Profile:
                    GetProfileData(item, cacheKey.Item);
                    var profileExpiration = GetExpirationFrom(UserSettings.ProfileExpiration);
                    if (UserSettings.ProfileInvalidation)
                    {
                        _cacheManager.SaveEntry(cacheKey.Key, item, profileExpiration, cacheKey.Date, cacheKey.Size, _fileManager.TempFilePath);
                    }
                    else
                    {
                        _cacheManager.SaveEntry(cacheKey.Key, item, profileExpiration);
                        File.Delete(_fileManager.TempFilePath);
                    }
                    item.IsCached = true;
                    break;
                case TitleType.Game:
                    if (item.Type == ItemType.File)
                    {
                        GetGameDataFromGpd(item);
                        _cacheManager.SaveEntry(cacheKey.Key, item, GetExpirationFrom(UserSettings.RecognizedGameExpiration));
                    } 
                    else
                    {
                        var gameExpiration = GetGameData(item)
                                                 ? UserSettings.RecognizedGameExpiration
                                                 : UserSettings.UseJqe360 && GetGameDataFromJqe360(item)
                                                       ? UserSettings.PartiallyRecognizedGameExpiration
                                                       : UserSettings.UnrecognizedGameExpiration;
                        _cacheManager.SaveEntry(cacheKey.Key, item, GetExpirationFrom(gameExpiration));
                    }
                    item.IsCached = true;
                    break;
                case TitleType.Content:
                    _cacheManager.SaveEntry(cacheKey.Key, item);
                    item.IsCached = true;
                    break;
                case TitleType.Unknown:
                    if (item.Type == ItemType.File)
                    {
                        try
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
                                    _cacheManager.SaveEntry(cacheKey.Key, item, svodExpiration, item.Date, item.Size);
                                }
                                else
                                {
                                    _cacheManager.SaveEntry(cacheKey.Key, item, svodExpiration);
                                }
                            }
                            else
                            {
                                _cacheManager.SaveEntry(cacheKey.Key, null, GetExpirationFrom(UserSettings.UnknownContentExpiration));
                            }
                        } 
                        catch
                        {
                            item.IsLocked = true;
                            item.LockMessage = ACCESSERRORMESSAGE;
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

        public ProfileItemWrapper GetProfileItem(FileSystemItem item)
        {
            var profilePath = string.Format("{1}FFFE07D1{2}00010000{2}{0}", item.Name, item.Path, _fileManager.Slash);
            string message = null;
            if (!_profileFileCache.ContainsKey(profilePath))
            {
                var profileItem = _fileManager.GetItemInfo(profilePath);
                if (profileItem != null)
                {
                    if (profileItem.Type == ItemType.File)
                    {
                        RecognizeType(profileItem);
                    }
                    else
                    {
                        profileItem = null;
                        message = "Profile is currently in use. Please sign out.";
                    }
                } 
                else
                {
                    message = "Profile file does not exists.";
                }
                _profileFileCache.Add(profilePath, profileItem);
            }
            return new ProfileItemWrapper(profilePath, _profileFileCache[profilePath], message);
        }

        private CacheComplexKey GetCacheKey(FileSystemItem item)
        {
            var key = new CacheComplexKey();
            switch (item.TitleType)
            {
                case TitleType.SystemDir:
                case TitleType.SystemFile:
                case TitleType.Content:
                    key.Item = item;
                    key.Key = item.Name;
                    break;
                case TitleType.Game:
                    key.Item = item;
                    key.Key = item.Type == ItemType.File ? item.Name.Replace(".gpd", string.Empty) : item.Name;
                    break;
                case TitleType.Profile:
                    if (item.Type == ItemType.File)
                    {
                        key.Item = item;
                        key.Key = item.FullPath;
                    } 
                    else
                    {
                        var recognition = GetProfileItem(item);
                        key.Item = recognition.Item;
                        key.Key = recognition.Path;
                        key.ErrorMessage = recognition.ErrorMessage;
                    }
                    break;
                case TitleType.Unknown:
                    key.Item = item;
                    key.Key = item.FullPath;
                    break;
            }
            if (key.Item != null)
            {
                key.Size = key.Item.Size;
                key.Date = key.Item.Date;
            }
            return key;
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
                item.RecognitionState = RecognitionState.Recognized;
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
                    try
                    {
                        var fileContent = _fileManager.ReadFileHeader(file.Path);
                        var svod = ModelFactory.GetModel<SvodPackage>(fileContent);
                        if (svod.IsValid)
                        {
                            item.Title = svod.TitleName;
                            item.Thumbnail = svod.ThumbnailImage;
                            item.ContentType = svod.ContentType;
                            item.RecognitionState = RecognitionState.Recognized;
                            infoFileFound = true;
                        }
                    }
                    catch {}
                }
            }
            return infoFileFound;
        }

        private bool GetGameDataFromJqe360(FileSystemItem item)
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
            if (result)
            {
                UIThread.Run(() =>
                                 {
                                     const string message = "<b>One or more titles couldn't be recognized fully.</b> If you played with them earlier try to extract the corresponding data from your profile by right clicking and selecting <i>Recognize Titles from Profile</i>.";
                                     _eventAggregator.GetEvent<NotifyUserMessageEvent>().Publish(new NotifyUserMessageEventArgs(message, MessageIcon.Info));
                                 });
                item.Title = title;
                item.RecognitionState = RecognitionState.PartiallyRecognized;
            }
            item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
            return result;
        }

        private bool GetGameDataFromGpd(FileSystemItem item)
        {
            try
            {
                var fileContent = _fileManager.ReadFileContent(item.Path, false, item.Size ?? 0);
                var gpd = ModelFactory.GetModel<GameFile>(fileContent);
                gpd.Parse();
                if (gpd.Strings.Count > 0) item.Title = gpd.Strings.First().Text;
                item.Thumbnail = gpd.Thumbnail;
                item.RecognitionState = RecognitionState.Recognized;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UpdateCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            _cacheManager.UpdateEntry(cacheKey.Key, item);
        }

        public void ThrowCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            _cacheManager.ClearCache(cacheKey.Key);
        }

        public string GetTempFilePath(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            var cacheEntry = _cacheManager.GetEntry(cacheKey);
            return cacheEntry != null && !string.IsNullOrEmpty(cacheEntry.TempFilePath) ? cacheEntry.TempFilePath : null;
        }

        public CacheEntry<FileSystemItem> GetCacheEntry(string key)
        {
            return _cacheManager.GetEntry(key);
        }
    }
}