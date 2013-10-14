﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
        private CacheManager _cacheManager;
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

        public TitleRecognizer(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public bool IsXboxFolder(FileSystemItem item)
        {
            return !string.IsNullOrEmpty(item.Name) && _recognitionKeywords.Any(r => new Regex(r.Pattern).IsMatch(item.Name));
        }

        public FileSystemItem RecognizeTitle(string itemPath)
        {
            var item = _fileManager.GetFileInfo(itemPath);
            RecognizeTitle(item);
            return item;
        }

        public void RecognizeTitle(FileSystemItem item, bool refresh = false)
        {
            var cacheKey = GetCacheKey(item);
            var hasCache = !refresh && _cacheManager.HasEntry(cacheKey);
            if (hasCache)
            {
                var cachedItem = _cacheManager.GetEntry<FileSystemItem>(cacheKey);
                if (cachedItem != null)
                {
                    item.Title = cachedItem.Title;
                    item.TitleType = cachedItem.TitleType;
                    item.ContentType = cachedItem.ContentType;
                    item.Thumbnail = cachedItem.Thumbnail;
                }
            }
            else
            {
                var recognition = _recognitionKeywords.FirstOrDefault(r => new Regex(r.Pattern).IsMatch(item.Name));
                if (recognition != null)
                {
                    item.Title = recognition.Title;
                    item.TitleType = recognition.Type;
                    switch (recognition.Type)
                    {
                        case TitleType.Profile:
                            var profilePath = item.Type == ItemType.Directory
                                                  ? string.Format("{1}FFFE07D1/00010000/{0}", item.Name, item.Path)
                                                  : item.Path;
                            var md5 = MD5.Create();
                            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(profilePath));
                            var tmpPath = string.Format(@"{0}\{1}", AppDomain.CurrentDomain.GetData("DataDirectory"), hash.ToHex());
                            
                            if (GetProfileData(item, profilePath, tmpPath))
                                _cacheManager.SaveEntry(cacheKey, item, DateTime.Now.AddDays(14), tmpPath);
                            break;
                        case TitleType.Game:
                            if (GetGameData(item) || GetGameDataFromJqe360(item))
                                _cacheManager.SaveEntry(cacheKey, item);
                            else
                                _cacheManager.SaveEntry(cacheKey, null, DateTime.Now.AddDays(7));
                            break;
                        case TitleType.Content:
                            var content = BitConverter.ToInt32(item.Name.FromHex(), 0);
                            if (Enum.IsDefined(typeof(ContentType), content))
                            {
                                item.ContentType = (ContentType) content;
                                item.Title = string.Format("{0}s", EnumHelper.GetStringValue(item.ContentType));
                            }
                            break;
                    }
                } 
                else if (item.Type == ItemType.File)
                {
                    var header = _fileManager.ReadFileHeader(item.Path);
                    var svod = ModelFactory.GetModel<SvodPackage>(header);
                    if (svod.IsValid)
                    {
                        item.Title = svod.DisplayName;
                        item.Thumbnail = svod.ThumbnailImage;
                        item.ContentType = svod.ContentType;
                        _cacheManager.SaveEntry(cacheKey, item, DateTime.Now.AddDays(14));
                    }
                    else
                    {
                        _cacheManager.SaveEntry(cacheKey, null);
                    }
                }    
            }
        }

        private string GetCacheKey(FileSystemItem item)
        {
            switch (item.TitleType)
            {
                case TitleType.Game:
                    return item.Name;
                default:
                    return item.FullPath;
            }            
        }

        private bool GetProfileData(FileSystemItem item, string profilePath, string tmpPath)
        {
            if (!_fileManager.FileExists(profilePath)) return false;

            _fileManager.ReadFileContent(profilePath, tmpPath);
            var stfs = ModelFactory.GetModel<StfsPackage>(tmpPath);
            stfs.ExtractAccount();
            item.Title = stfs.Account.GamerTag;
            item.Thumbnail = stfs.ThumbnailImage;
            item.ContentType = stfs.ContentType;
            return true;
        }

        private bool GetGameData(FileSystemItem item)
        {
            var infoFileFound = false;

            var systemdir = item.Name.StartsWith("5841") ? "000D0000" : "00007000";

            //TODO
            var drive = "Hdd1";

            var gamePath = string.Format("/{1}/Content/0000000000000000/{0}/{2}/", item.Name, drive, systemdir);
            if (_fileManager.FolderExists(gamePath))
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

        //public bool HasCache(FileSystemItem item)
        //{
        //    return item.Type != ItemType.Drive && _cacheManager.HasEntry(GetCacheKey(item));
        //}

        public void UpdateCache(FileSystemItem item)
        {
            var cacheKey = GetCacheKey(item);
            _cacheManager.UpdateEntry(cacheKey, item);
        }

        public IDisposable BeginTransaction()
        {
            _cacheManager = new CacheManager();
            return _cacheManager;
        }
    }
}