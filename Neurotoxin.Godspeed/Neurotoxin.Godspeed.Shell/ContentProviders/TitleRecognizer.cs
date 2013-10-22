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

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    internal class TitleRecognizer
    {
        private readonly IFileManager _fileManager;
        private readonly CacheManager _cacheManager;
        private readonly Dictionary<string, FileSystemItem> _profileFileCache = new Dictionary<string, FileSystemItem>();

        private readonly List<RecognitionInformation> _recognitionKeywords = new List<RecognitionInformation>
            {
                new RecognitionInformation("^0000000000000000$", "Games", TitleType.SystemDir),
                new RecognitionInformation("^584E07D2$", "XNA Indie Player", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07C3$", "Gamer Pictures", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07D1$", "Profile Data", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07DF$", "Avatar Editor", TitleType.SystemDir),
                new RecognitionInformation("^F[0-9A-F]{7}$", "System Data", TitleType.SystemDir),
                new RecognitionInformation("^F[0-9A-F]{7}.gpd$", "System Data", TitleType.SystemFile, ItemType.File),
                new RecognitionInformation("^[1-9A-F][0-9A-F]{7}$", "Unknown Game", TitleType.Game),
                new RecognitionInformation("^[1-9A-F][0-9A-F]{7}.gpd$", "Unknown Game", TitleType.Game, ItemType.File),
                new RecognitionInformation("^[0-9A-F]{8}$", "Unknown Content", TitleType.Content),
                new RecognitionInformation("^E0000[0-9A-F]{11}$", "Unknown Profile", TitleType.Profile, ItemType.Directory | ItemType.File),
            };

        public TitleRecognizer(IFileManager fileManager, CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _fileManager = fileManager;
        }

        public bool IsXboxFolder(FileSystemItem item)
        {
            if (string.IsNullOrEmpty(item.Name)) return false;
            if (item.Type != ItemType.Directory) return false;
            return _recognitionKeywords.Any(r => r.ItemTypeFlags.HasFlag(ItemType.Directory) && new Regex(r.Pattern, RegexOptions.IgnoreCase).IsMatch(item.Name));
        }

        public bool RecognizeType(FileSystemItem item)
        {
            var recognition = _recognitionKeywords.FirstOrDefault(r => new Regex(r.Pattern, RegexOptions.IgnoreCase).IsMatch(item.Name));
            if (recognition == null || !recognition.ItemTypeFlags.HasFlag(item.Type)) return false;
            item.Title = recognition.Title;
            item.TitleType = recognition.TitleType;
            return true;
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

        public void RecognizeTitle(FileSystemItem item, bool overwrite = false)
        {
            if (item.TitleType == TitleType.SystemDir || item.TitleType == TitleType.SystemFile) return;
            var cacheItem = GetCacheItem(item);
            if (!overwrite && MergeWithCachedEntry(item, cacheItem)) return;

            var cacheKey = GetCacheKey(cacheItem);
            if (overwrite) _cacheManager.ClearCache(cacheKey);

            switch (item.TitleType)
            {
                case TitleType.Profile:
                    if (cacheItem.Type == ItemType.File) GetProfileData(item, cacheItem.Path);
                    _cacheManager.SaveEntry(cacheKey, item, DateTime.Now.AddDays(14), cacheItem.Date, cacheItem.Size, _fileManager.TempFilePath);
                    item.IsCached = true;
                    break;
                case TitleType.Game:
                    if (item.Type == ItemType.File)
                    {
                        GetGameDataFromGpd(item);
                        _cacheManager.SaveEntry(cacheKey, item);
                    } 
                    else
                    {
                        if (GetGameData(item) || GetGameDataFromJqe360(item))
                            _cacheManager.SaveEntry(cacheKey, item);
                        else
                            _cacheManager.SaveEntry(cacheKey, item, DateTime.Now.AddDays(7));
                    }
                    item.IsCached = true;
                    break;
                case TitleType.Content:
                    var content = BitConverter.ToInt32(item.Name.FromHex(), 0);
                    if (Enum.IsDefined(typeof (ContentType), content))
                    {
                        item.ContentType = (ContentType) content;
                        var title = EnumHelper.GetStringValue(item.ContentType);
                        item.Title = string.Format("{0}{1}", title, title.EndsWith("data", StringComparison.InvariantCultureIgnoreCase) ? string.Empty : "s");
                    }
                    else
                    {
                        //TODO: log unknown entry
                    }
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
                            _cacheManager.SaveEntry(cacheKey, item, DateTime.Now.AddDays(14), item.Date, item.Size);
                        }
                        else
                        {
                            _cacheManager.SaveEntry(cacheKey, null);
                        }
                        item.IsCached = true;
                    }
                    break;
            }
        }

        private FileSystemItem GetCacheItem(FileSystemItem item)
        {
            if (item.TitleType == TitleType.Profile && item.Type == ItemType.Directory)
            {
                var profilePath = string.Format("{1}FFFE07D1{2}00010000{2}{0}", item.Name, item.Path, _fileManager.Slash);
                if (_profileFileCache.ContainsKey(profilePath)) return _profileFileCache[profilePath];
                if (_fileManager.FileExists(profilePath))
                {
                    var profileItem = _fileManager.GetFileInfo(profilePath);
                    if (profileItem == null)
                    {
                        throw new NotImplementedException("Please sign out");
                    }
                    RecognizeType(profileItem);
                    _profileFileCache.Add(profilePath, profileItem);
                    return profileItem;
                }
            }
            return item;
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

        private bool GetProfileData(FileSystemItem item, string profilePath)
        {
            try
            {
                var bytes = _fileManager.ReadFileContent(profilePath);
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
            var fileContent = _fileManager.ReadFileContent(item.Path);
            var gpd = ModelFactory.GetModel<GpdFile>(fileContent);
            gpd.Parse();
            if (gpd.Strings.Count > 0) item.Title = gpd.Strings.First().Text;
            if (gpd.Images.Count > 0) item.Thumbnail = gpd.Images.First().ImageData;
        }

        public void UpdateCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            _cacheManager.UpdateEntry(cacheKey, item);
        }

        public void ThrowCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
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