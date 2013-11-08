using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class StfsPackageContent : IFileManager, IDisposable
    {
        private const char SLASH = '\\';
        public char Slash
        {
            get { return SLASH; }
        }

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
            var list = folder.Folders.Select(f => CreateModel(f, string.Format(@"{0}{1}\", path, f.Name))).ToList();
            list.AddRange(folder.Files.Select(f => CreateModel(f, string.Format(@"{0}{1}", path, f.Name))));

            return list;
        }

        public FileSystemItem GetFolderInfo(string path)
        {
            return GetFolderInfo(path, ItemType.Drive);
        }

        public FileSystemItem GetFolderInfo(string path, ItemType type)
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
            var bytes = ReadFileContent(remotePath);
            var offset = (int)remoteStartPosition;
            fs.Write(bytes, offset, bytes.Length - offset);
        }

        public void AddFile(string targetPath, string sourcePath)
        {
            var content = File.ReadAllBytes(sourcePath);
            _stfs.AddFile(targetPath, content);
        }

        public void ReplaceFile(string targetPath, string sourcePath)
        {
            var content = File.ReadAllBytes(sourcePath);
            var fileEntry = _stfs.GetFileEntry(targetPath);
            _stfs.ReplaceFile(fileEntry, content);
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