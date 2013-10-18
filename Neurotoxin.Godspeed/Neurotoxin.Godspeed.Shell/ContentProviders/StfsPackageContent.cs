using System;
using System.Collections.Generic;
using System.Linq;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class StfsPackageContent : IFileManager
    {
        private StfsPackage _stfs;

        public string TempFilePath { get; set; }

        public List<FileSystemItem> GetDrives()
        {
            const string path = @"\Root\";
            return new List<FileSystemItem>
                       {
                           new FileSystemItem
                               {
                                   Name = _stfs.DisplayName,
                                   Path = path,
                                   FullPath = string.Format(@"{0}:\{1}", _stfs.DisplayName, path),
                                   Type = ItemType.Drive,
                                   Thumbnail = _stfs.ThumbnailImage
                               }
                       };
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();

            var folder = _stfs.GetFolderEntry(path);
            var list = folder.Folders.Select(f => new FileSystemItem
            {
                Name = f.Name,
                Type = ItemType.Directory,
                Path = string.Format(@"{0}{1}\", path, f.Name),
                FullPath = string.Format(@"{0}:\{1}{2}\", _stfs.DisplayName, path, f.Name),
                Date = DateTimeExtensions.FromFatFileTime(f.AccessTimeStamp)
            }).ToList();
            list.AddRange(folder.Files.Select(f => new FileSystemItem
            {
                Name = f.Name,
                Type = ItemType.File,
                Path = string.Format("{0}{1}", path, f.Name),
                FullPath = string.Format(@"{0}:\{1}{2}", _stfs.DisplayName, path, f.Name),
                Date = DateTimeExtensions.FromFatFileTime(f.AccessTimeStamp),
                Size = f.FileSize
            }));

            return list;
        }

        public FileSystemItem GetFileInfo(string path)
        {
            var f = _stfs.GetFileEntry(path);
            return new FileSystemItem
            {
                Name = f.Name,
                Type = ItemType.File,
                Path = path,
                FullPath = string.Format(@"{0}:\{1}", _stfs.DisplayName, path),
                Date = DateTimeExtensions.FromFatFileTime(f.AccessTimeStamp),
                Size = f.FileSize,
            };
        }

        public DateTime GetFileModificationTime(string path)
        {
            var f = _stfs.GetFileEntry(path);
            return DateTimeExtensions.FromFatFileTime(f.AccessTimeStamp);
        }

        public bool DriveIsReady(string drive)
        {
            return true;
        }

        public bool FileExists(string path)
        {
            return _stfs.GetFileEntry(path, true) != null;
        }

        public bool FolderExists(string path)
        {
            return _stfs.GetFolderEntry(path, true) != null;
        }

        public void DeleteFolder(string path)
        {
            _stfs.RemoveFolder(path);
        }

        public void DeleteFile(string path)
        {
            _stfs.RemoveFile(path);
        }

        public void CreateFolder(string path)
        {
            _stfs.AddFolder(path);
        }

        public byte[] ReadFileContent(string path)
        {
            return _stfs.ExtractFile(path);
        }

        public byte[] ReadFileHeader(string path)
        {
            return _stfs.ExtractFile(path, 0x971A);
        }

        public Account GetAccount()
        {
            if (_stfs.Account == null) _stfs.ExtractAccount();
            return _stfs.Account;
        }

        public void LoadPackage(byte[] bytes)
        {
            _stfs = ModelFactory.GetModel<StfsPackage>(bytes);
        }
    }
}