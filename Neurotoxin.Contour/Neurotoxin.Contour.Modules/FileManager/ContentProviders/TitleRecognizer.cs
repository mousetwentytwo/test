using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Core.Extensions;
using Neurotoxin.Contour.Core.Io.Stfs;
using Neurotoxin.Contour.Core.Models;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Presentation.Extensions;

namespace Neurotoxin.Contour.Modules.FileManager.ContentProviders
{
    internal class TitleRecognizer
    {
        private readonly IFileManager _fileManager;
        private readonly CacheManager _cacheManager;

        private readonly List<RecognitionInformation> _recognitionKeywords = new List<RecognitionInformation>
            {
                new RecognitionInformation("^0000000000000000$", "Games", TitleType.SystemDir),
                new RecognitionInformation("^584E07D2$", "XNA Indie Player", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07C3$", "Gamer Pictures", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07D1$", "Profile Data", TitleType.SystemDir),
                new RecognitionInformation("^FFFE07DF$", "Avatar Editor", TitleType.SystemDir),
                new RecognitionInformation("^FFFE[0-9A-F]{4}$", "System Data", TitleType.SystemDir),
                new RecognitionInformation("^[1-9A-F][0-9A-F]{7}$", "Unknown Game", TitleType.Game),
                new RecognitionInformation("^[0-9A-F]{8}$", "Unknown Content", TitleType.Content),
                new RecognitionInformation("^E0000[0-9A-F]{11}$", "Unknown Profile", TitleType.Profile),
            };

        public TitleRecognizer(IFileManager fileManager, CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _fileManager = fileManager;
        }

        public bool IsXboxFolder(FileSystemItem item)
        {
            return !string.IsNullOrEmpty(item.Name) &&
                   _recognitionKeywords.Any(r => new Regex(r.Pattern).IsMatch(item.Name));
        }

        public bool RecognizeType(FileSystemItem item)
        {
            var recognition = _recognitionKeywords.FirstOrDefault(r => new Regex(r.Pattern).IsMatch(item.Name));
            if (recognition == null) return false;
            item.Title = recognition.Title;
            item.TitleType = recognition.Type;
            return true;
        }

        public bool MergeWithCachedEntry(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            if (_cacheManager.HasEntry(cacheKey, item.Size, item.Date))
            {
                var cachedItem = _cacheManager.GetEntry(cacheKey);
                if (cachedItem != null)
                {
                    item.Title = cachedItem.Content.Title;
                    item.TitleType = cachedItem.Content.TitleType;
                    item.ContentType = cachedItem.Content.ContentType;
                    item.Thumbnail = cachedItem.Content.Thumbnail;
                }
                return true;
            }
            return false;
        }

        public void RecognizeTitle(FileSystemItem item, bool overwrite = false)
        {
            if (item.TitleType == TitleType.SystemDir) return;

            var cacheKey = GetCacheKey(item);
            var hasCached = !overwrite && MergeWithCachedEntry(item);
            if (overwrite) _cacheManager.ClearCache(cacheKey);
            if (hasCached) return;

            switch (item.TitleType)
            {
                case TitleType.Profile:
                    string profilePath;
                    DateTime date;
                    long? size;
                    if (item.Type == ItemType.Directory)
                    {
                        profilePath = string.Format("{1}FFFE07D1/00010000/{0}", item.Name, item.Path);
                        if (!_fileManager.FileExists(profilePath)) break;
                        var fileItem = _fileManager.GetFileInfo(profilePath);
                        date = fileItem.Date;
                        size = fileItem.Size;
                    }
                    else
                    {
                        profilePath = item.Path;
                        date = item.Date;
                        size = item.Size;
                    }
                    if (GetProfileData(item, profilePath))
                        _cacheManager.SaveEntry(cacheKey, item, DateTime.Now.AddDays(14), date, size, _fileManager.TempFilePath);
                    break;
                case TitleType.Game:
                    if (GetGameData(item) || GetGameDataFromJqe360(item))
                        _cacheManager.SaveEntry(cacheKey, item);
                    else
                        _cacheManager.SaveEntry(cacheKey, null, DateTime.Now.AddDays(7));
                    break;
                case TitleType.Content:
                    var content = BitConverter.ToInt32(item.Name.FromHex(), 0);
                    if (Enum.IsDefined(typeof (ContentType), content))
                    {
                        item.ContentType = (ContentType) content;
                        item.Title = string.Format("{0}s", EnumHelper.GetStringValue(item.ContentType));
                    }
                    else
                    {
                        //TODO: log unknown entry
                    }
                    _cacheManager.SaveEntry(cacheKey, item);
                    break;
                case TitleType.Undefined:
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
                    }
                    break;
            }
        }

        private string GetCacheKey(FileSystemItem item)
        {
            switch (item.TitleType)
            {
                case TitleType.SystemDir:
                case TitleType.Content:
                case TitleType.Game:
                    return item.Name;
                case TitleType.Profile:
                    var slash = item.FullPath.Contains("/") ? "/" : "\\";
                    return item.Type == ItemType.Directory
                               ? string.Format("{1}FFFE07D1{2}00010000{2}{0}", item.Name, item.FullPath, slash)
                               : item.FullPath;
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
                    var regex = new Regex(string.Format("Title: .*?>(.*?)<.*?TitleID: {0}", item.Name),
                                          RegexOptions.IgnoreCase);
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

        public void UpdateCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            _cacheManager.UpdateEntry(cacheKey, item);
        }

        public byte[] ReadFileContent(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            var cacheEntry = _cacheManager.GetEntry(cacheKey, item.Size, item.Date);
            return cacheEntry != null && !string.IsNullOrEmpty(cacheEntry.TempFilePath)
                ? File.ReadAllBytes(cacheEntry.TempFilePath) 
                : _fileManager.ReadFileContent(item.Path);
        }
    }
}