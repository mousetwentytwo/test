using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class StfsPackageContent : IFileManager, IDisposable
    {
        public string TempFilePath { get; set; }

        private const char SLASH = '\\';
        public char Slash
        {
            get { return SLASH; }
        }

        private ContentType _contentType;
        private StfsPackage _stfs;
        private readonly IEventAggregator _eventAggregator;

        public StfsPackageContent(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

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
                                   Thumbnail = _stfs.ThumbnailImage,
                                   ContentType = _contentType
                               }
                       };
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();

            var folder = _stfs.GetFolderEntry(path);
            var list = folder.Folders.Select(f => CreateModel(f, string.Format(@"{0}{1}\", path, f.Name))).ToList();
            list.AddRange(folder.Files.Select(f => CreateModel(f, string.Format(@"{0}{1}", path, f.Name))));

            return list;
        }

        public FileSystemItem GetItemInfo(string path)
        {
            return GetItemInfo(path, null);
        }

        public FileSystemItem GetItemInfo(string path, ItemType? type)
        {
            return GetItemInfo(path, type, true);
        }

        public FileSystemItem GetItemInfo(string path, ItemType? type, bool swallowException)
        {
            var item = GetFileInfo(path) ?? GetFolderInfo(path);
            if (item == null) return null;
            if (type != null)
            {
                if ((type == ItemType.File && item.Type != ItemType.File) || 
                    (type != ItemType.File && item.Type != ItemType.Directory)) return null;
                item.Type = type.Value;
            }
            return item;
        }

        private FileSystemItem GetFolderInfo(string path)
        {
            if (!path.EndsWith("\\")) path += "\\";
            return CreateModel(_stfs.GetFolderEntry(path), path);
        }

        public FileSystemItem GetFileInfo(string path)
        {
            return CreateModel(_stfs.GetFileEntry(path), path);
        }

        private FileSystemItem CreateModel(FileEntry f, string path)
        {
            return new FileSystemItem
            {
                Name = f.Name,
                Type = f.IsDirectory ? ItemType.Directory : ItemType.File,
                Path = path,
                FullPath = string.Format(@"{0}:\{1}", _stfs.DisplayName, path),
                Date = DateTimeExtensions.FromFatFileTime(f.AccessTimeStamp),
                Size = f.FileSize
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

        public FileExistenceInfo FileExists(string path)
        {
            var entry = _stfs.GetFileEntry(path, true);
            if (entry == null) return false;
            return entry.FileSize;
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

        public byte[] ReadFileContent(string path, bool saveTempFile, long fileSize)
        {
            return ReadFileContent(path);
        }

        public byte[] ReadFileHeader(string path)
        {
            return _stfs.ExtractFile(path, 0x971A);
        }

        public void ExtractFile(string remotePath, FileStream fs, long remoteStartPosition)
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(0, 0, 0, 0));
            var bytes = ReadFileContent(remotePath);
            var offset = (int)remoteStartPosition;
            fs.Write(bytes, offset, bytes.Length - offset);
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(100, bytes.Length, bytes.Length, 0));
        }

        public void AddFile(string targetPath, string sourcePath)
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(0, 0, 0, 0));
            var content = File.ReadAllBytes(sourcePath);
            _stfs.AddFile(targetPath, content);
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(100, content.Length, content.Length, 0));
        }

        public void ReplaceFile(string targetPath, string sourcePath)
        {
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(0, 0, 0, 0));
            var content = File.ReadAllBytes(sourcePath);
            var fileEntry = _stfs.GetFileEntry(targetPath);
            _stfs.ReplaceFile(fileEntry, content);
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(100, content.Length, content.Length, 0));
        }

        public FileSystemItem Rename(string path, string newName)
        {
            var entry = _stfs.Rename(path, newName);
            var oldName = Path.GetFileName(path.TrimEnd('\\'));
            var r = new Regex(string.Format(@"{0}\\?$", Regex.Escape(oldName)), RegexOptions.IgnoreCase);
            var newPath = r.Replace(path, newName);
            return CreateModel(entry, newPath);
        }

        public Account GetAccount()
        {
            if (_stfs.Account == null) _stfs.ExtractAccount();
            return _stfs.Account;
        }

        public void LoadPackage(BinaryContent content)
        {
            _contentType = content.ContentType;
            _stfs = ModelFactory.GetModel<StfsPackage>(content.Content);
        }

        public byte[] Save()
        {
            return _stfs.Save();
        }


        public void Dispose()
        {
            _stfs = null;
            GC.Collect();
        }
    }
}