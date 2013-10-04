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

        public TitleManager(T fileManager)
        {
            _binaryFormatter = new BinaryFormatter();
            _fileManager = fileManager;
        }

        public void RecognizeTitle(FileSystemItem item, FileSystemItemViewModel parentFolder)
        {
            var tmpPath = GetTempFileName(item);
            var cachedItem = LoadCache(tmpPath);
            var hasCache = cachedItem != null;
            if (hasCache)
            {
                item.Subtype = cachedItem.Subtype;
                item.Title = cachedItem.Title;
                item.Thumbnail = cachedItem.Thumbnail;
            }
            else
            {
                RecognizeTitle(item, parentFolder, tmpPath);
            }

            //TODO: refactor TitleId property
            var name = item.TitleId ?? item.Title;

            switch (name)
            {
                case "Content":
                    item.Title = "[Content]";
                    item.TitleId = null;
                    item.Subtype = ItemSubtype.Content;
                    break;
                case "0000000000000000":
                    item.Title = "[Games]";
                    item.TitleId = null;
                    item.Subtype = ItemSubtype.GamesFolder;
                    break;
                case "00000002":
                    item.Title = "[Downloadable Contents]";
                    item.Subtype = ItemSubtype.DownloadableContents;
                    break;
                case "00007000":
                    item.Title = "[GOD Contents]";
                    item.Subtype = ItemSubtype.GameOnDemand;
                    break;
                case "00009000":
                    item.Title = "[Avatar Items]";
                    item.Subtype = ItemSubtype.AvatarItems;
                    break;
                case "000D0000":
                    item.Title = "[XBLA Contents]";
                    item.Subtype = ItemSubtype.XboxLiveArcadeGame;
                    break;
                case "000B0000":
                    item.Title = "[Title Updates]";
                    item.Subtype = ItemSubtype.TitleUpdates;
                    break;
                case "584E07D2":
                    item.Title = "XNA Indie Player";
                    break;
                default:
                    if (name.StartsWith("FFFE"))
                    {
                        item.Title = "[System Data]";
                        item.Subtype = ItemSubtype.Undefined;
                        item.Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                    }
                    break;
            }
        }

        private void RecognizeTitle(FileSystemItem item, FileSystemItemViewModel parentFolder, string tmpPath)
        {
            switch (parentFolder.Subtype)
            {
                case ItemSubtype.Content:
                    var profilePath = string.Format("{1}{0}/FFFE07D1/00010000/{0}", item.TitleId, parentFolder.Path);
                    if (GetProfileData(item, profilePath)) SaveCache(item, tmpPath);
                    break;
                case ItemSubtype.GamesFolder:
                    item.Subtype = ItemSubtype.Game;
                    if (GetGameData(item) || GetGameDataFromJqe360(item)) SaveCache(item, tmpPath);
                    break;
                case ItemSubtype.Profile:
                    item.Subtype = ItemSubtype.GameSaves;
                    if (GetGameData(item) || GetGameDataFromJqe360(item)) SaveCache(item, tmpPath);
                    break;
                case ItemSubtype.TitleUpdates:
                    item.Subtype = ItemSubtype.TitleUpdate;
                    break;
                case ItemSubtype.DownloadableContents:
                    item.Subtype = ItemSubtype.DownloadableContent;
                    break;
            }

            if (item.Type == ItemType.File)
            {
                switch (item.Subtype)
                {
                    case ItemSubtype.Profile:
                        GetProfileData(item, item.Path);
                        break;
                    default:
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
        }

        private string GetTempFileName(FileSystemItem item)
        {
            switch (item.Type)
            {
                case ItemType.Directory:
                    return string.Format("tmp/{0}", item.TitleId);
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

            var systemdir = item.TitleId.StartsWith("5841") ? "000D0000" : "00007000";

            //TODO
            var Drive = "Hdd1";

            var gamePath = string.Format("/{1}/Content/0000000000000000/{0}/{2}/", item.TitleId, Drive, systemdir);
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
            var request = WebRequest.Create(string.Format("http://covers.jqe360.com/main.php?search={0}", item.TitleId));
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
                    var regex = new Regex(string.Format("Title: .*?>(.*?)<.*?TitleID: {0}", item.TitleId), RegexOptions.IgnoreCase);
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

        private void SaveCache(FileSystemItem fileSystemItem, string path)
        {
            var fs = new FileStream(path, FileMode.Create);
            _binaryFormatter.Serialize(fs, fileSystemItem);
            fs.Flush();
            fs.Close();
        }

        private FileSystemItem LoadCache(string path)
        {
            if (!File.Exists(path)) return null;
            var fs = new FileStream(path, FileMode.Open);
            var cachedItem = (FileSystemItem)_binaryFormatter.Deserialize(fs);
            fs.Close();
            return cachedItem;
        }

    }
}