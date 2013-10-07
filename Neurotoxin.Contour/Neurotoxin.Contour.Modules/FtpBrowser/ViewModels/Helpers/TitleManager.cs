using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Neurotoxin.Contour.Core.Extensions;
using Neurotoxin.Contour.Core.Io.Stfs;
using Neurotoxin.Contour.Core.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers
{
    internal class TitleManager<T> where T : IFileManager
    {
        private readonly BinaryFormatter _binaryFormatter;
        private readonly T _fileManager;
        private readonly Dictionary<string, ItemSubtype> _recognitionKeywords = new Dictionary<string, ItemSubtype>
        {
            {"^Content$", ItemSubtype.Content},
            {"^0000000000000000$", ItemSubtype.GamesFolder},
            {"^00000001$", ItemSubtype.SaveData},
            {"^00000002$", ItemSubtype.DownloadableContents},
            {"^00004000$", ItemSubtype.NXEData},
            {"^00007000$", ItemSubtype.GODData},
            {"^00009000$", ItemSubtype.AvatarItems},
            {"^00010000$", ItemSubtype.ProfileData},
            {"^00020000$", ItemSubtype.GamerPictures},
            {"^00030000$", ItemSubtype.Themes},
            {"^00080000$", ItemSubtype.XboxLiveArcadeGame}, //demos?!
            {"^00090000$", ItemSubtype.Videos},
            {"^000B0000$", ItemSubtype.TitleUpdates},
            {"^000D0000$", ItemSubtype.XboxLiveArcadeGame},
            {"^584E07D2$", ItemSubtype.IndiePlayer},
            {"^FFFE[0-9A-F]{4}$", ItemSubtype.SystemData},
            {"^[0-9A-F]{8}$", ItemSubtype.Game},
            {"^E00001[0-9A-F]{10}$", ItemSubtype.Profile},
        };

        public TitleManager(T fileManager)
        {
            _binaryFormatter = new BinaryFormatter();
            _fileManager = fileManager;
        }

        public bool IsXboxFolder(FileSystemItem item)
        {
            return !string.IsNullOrEmpty(item.Name) && _recognitionKeywords.Keys.Any(key => new Regex(key).IsMatch(item.Name));
        }

        public void RecognizeTitle(FileSystemItem item, FileSystemItem parentFolder, bool refresh = false)
        {
            var tmpPath = GetTempFileName(item);
            var cachedItem = !refresh ? LoadCache(tmpPath) : null;
            var hasCache = cachedItem != null;
            if (hasCache)
            {
                item.Subtype = cachedItem.Subtype;
                item.Title = cachedItem.Title;
                item.Thumbnail = cachedItem.Thumbnail;
                if (item.Subtype == ItemSubtype.Game && parentFolder.Subtype == ItemSubtype.Profile) item.Subtype = ItemSubtype.GameSaves;
            }
            else
            {
                var subtypeKey = _recognitionKeywords.Keys.FirstOrDefault(key => new Regex(key).IsMatch(item.Name));
                if (subtypeKey != null)
                {
                    item.Subtype = _recognitionKeywords[subtypeKey];
                    switch (item.Subtype)
                    {
                        case ItemSubtype.SystemData:
                            switch (item.Name)
                            {
                                case "FFFE07C3":
                                    item.Title = "Gamer Pictures";
                                    break;
                                case "FFFE07D1":
                                    item.Title = "Profile Data";
                                    break;
                                case "FFFE07DF":
                                    item.Title = "Avatar Editor";
                                    break;
                                default:
                                    item.Title = EnumHelper.GetStringValue(item.Subtype);
                                    break;
                            }
                            break;
                        default:
                            item.Title = EnumHelper.GetStringValue(item.Subtype);
                            break;
                    }
                }
                RecognizeTitle(item, parentFolder, tmpPath);
            }
        }

        private void RecognizeTitle(FileSystemItem item, FileSystemItem parentFolder, string tmpPath)
        {
            switch (item.Subtype)
            {
                case ItemSubtype.Profile:
                    var profilePath = item.Type == ItemType.Directory
                                          ? string.Format("{1}/FFFE07D1/00010000/{0}", item.Name, item.Path)
                                          : item.Path;
                    if (GetProfileData(item, profilePath)) SaveCache(item, tmpPath);
                    return;
                case ItemSubtype.Game:
                    if (GetGameData(item) || GetGameDataFromJqe360(item)) SaveCache(item, tmpPath);
                    if (parentFolder.Subtype == ItemSubtype.Profile) item.Subtype = ItemSubtype.GameSaves;
                    return;
                default:
                    if (item.Type != ItemType.File) return;

                    switch (parentFolder.Subtype)
                    {
                        case ItemSubtype.TitleUpdates:
                            item.Subtype = ItemSubtype.TitleUpdate;
                            break;
                        case ItemSubtype.DownloadableContents:
                            item.Subtype = ItemSubtype.DownloadableContent;
                            break;
                    }

                    var header = _fileManager.ReadFileHeader(item.Path);
                    var svod = ModelFactory.GetModel<SvodPackage>(header);
                    if (svod.IsValid)
                    {
                        item.Title = svod.DisplayName;
                        item.Thumbnail = svod.ThumbnailImage;
                        SaveCache(item, tmpPath);
                    }
                    break;
            }
        }

        private string GetTempFileName(FileSystemItem item)
        {
            switch (item.Type)
            {
                case ItemType.Directory:
                    return string.Format("tmp/{0}", item.Name);
                case ItemType.File:
                    var md5 = MD5.Create();
                    var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(item.Path));
                    return string.Format("tmp/{0}", hash.ToHex());
                default:
                    throw new NotSupportedException();
            }
        }

        private bool GetProfileData(FileSystemItem item, string profilePath = null)
        {
            if (profilePath == null) profilePath = item.Path;
            if (!_fileManager.FileExists(profilePath)) return false;

            var fileContent = _fileManager.ReadFileContent(profilePath);
            var stfs = ModelFactory.GetModel<StfsPackage>(fileContent);
            stfs.ExtractAccount();
            item.Title = stfs.Account.GamerTag;
            item.Thumbnail = stfs.ThumbnailImage;
            item.Subtype = ItemSubtype.Profile;
            return true;
        }

        private bool GetGameData(FileSystemItem item)
        {
            var infoFileFound = false;

            var systemdir = item.Name.StartsWith("5841") ? "000D0000" : "00007000";

            //TODO
            var Drive = "Hdd1";

            var gamePath = string.Format("/{1}/Content/0000000000000000/{0}/{2}/", item.Name, Drive, systemdir);
            if (_fileManager.FolderExists(gamePath))
            {
                var file = _fileManager.GetList(gamePath).FirstOrDefault(i => i.Type == ItemType.File);
                if (file != null)
                {
                    //TODO: name instead of title
                    var fileContent = _fileManager.ReadFileHeader(string.Format("{0}{1}", gamePath, file.Title));
                    var svod = ModelFactory.GetModel<SvodPackage>(fileContent);
                    if (svod.IsValid)
                    {
                        item.Title = svod.TitleName;
                        item.Thumbnail = svod.ThumbnailImage;
                        item.Subtype = ItemSubtype.Game;
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
                    result = true;
                }
            }
            catch { }
            item.Title = title;
            item.Subtype = ItemSubtype.Game;
            item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/xbox_logo.png");
            return result;
        }

        public void SaveCache(FileSystemItem item, string path = null)
        {
            if (path == null) path = GetTempFileName(item);
            var fs = new FileStream(path, FileMode.Create);
            _binaryFormatter.Serialize(fs, item);
            fs.Flush();
            fs.Close();
        }

        public bool HasCache(FileSystemItem item)
        {
            return item.Type != ItemType.Drive && HasCache(GetTempFileName(item));
        }

        private bool HasCache(string path)
        {
            return File.Exists(path);
        }

        private FileSystemItem LoadCache(string path)
        {
            if (!HasCache(path)) return null;
            var fs = new FileStream(path, FileMode.Open);
            var cachedItem = (FileSystemItem)_binaryFormatter.Deserialize(fs);
            fs.Close();
            return cachedItem;
        }

    }
}