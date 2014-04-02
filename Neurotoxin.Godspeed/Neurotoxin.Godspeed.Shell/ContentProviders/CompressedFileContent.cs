using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using SharpCompress.Archive;
using SharpCompress.Archive.Rar;
using SharpCompress.Archive.SevenZip;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class CompressedFileContent : IFileManager, IDisposable
    {
        private string _archivePath;
        private IArchive _archive;
        private Tree<FileSystemItem> _fileStructure;
        private readonly IEventAggregator _eventAggregator;

        public string TempFilePath { get; set; }

        private const char SLASH = '/';
        private const char BACKSLASH = '\\';

        public char Slash
        {
            get { return _archive is RarArchive ? BACKSLASH : SLASH; }
        }

        public CompressedFileContent(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public List<FileSystemItem> GetDrives()
        {
            return new List<FileSystemItem>
                       {
                           new FileSystemItem
                               {
                                   Name = Path.GetFileName(_archivePath),
                                   Path = string.Empty,
                                   FullPath = string.Format(@"{0}:\", _archivePath),
                                   Type = ItemType.Drive,
                                   Thumbnail = ApplicationExtensions.GetContentByteArray("Resources/package.png")
                               }
                       };
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new ArgumentException();
            return _fileStructure.GetChildren(path);
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
            if (type.HasValue && type.Value == ItemType.Directory && _archive is RarArchive && path[path.Length -1] != BACKSLASH) path += BACKSLASH;
            FileSystemItem item;
            if (!_fileStructure.TryGetItem(path, out item)) return null;
            if (type != null)
            {
                if ((type == ItemType.File && item.Type != ItemType.File) ||
                    (type != ItemType.File && item.Type != ItemType.Directory)) return null;
                item.Type = type.Value;
            }
            return item;
        }

        private FileSystemItem CreateModel(IArchiveEntry entry)
        {
            DateTime date = _archive is SevenZipArchive
                                ? DateTime.MinValue
                                : (entry.LastModifiedTime ??
                                   entry.LastAccessedTime ??
                                   entry.CreatedTime ??
                                   entry.ArchivedTime ??
                                   DateTime.MinValue);

            return CreateModel(entry.FilePath, 
                               entry.IsDirectory,
                               date,
                               entry.IsDirectory ? (long?) null : entry.Size);
        }

        private FileSystemItem CreateModel(string path, bool isDirectory, DateTime date, long? size)
        {
            var slash = Slash;
            var name = path.TrimEnd(slash);
            var slashIndex = name.LastIndexOf(slash);
            if (slashIndex > -1) name = name.Substring(slashIndex + 1);

            if (isDirectory && path[path.Length - 1] != slash) path += slash;

            return new FileSystemItem
            {
                Name = name,
                Type = isDirectory ? ItemType.Directory : ItemType.File,
                Path = path,
                FullPath = string.Format(@"{0}:\{1}", _archivePath, path),
                Date = date,
                Size = size
            };
        }

        public DateTime GetFileModificationTime(string path)
        {
            var entry = _archive.Entries.First(e => e.FilePath == path);
            return entry.LastModifiedTime ?? DateTime.MinValue;
        }

        public bool DriveIsReady(string drive)
        {
            return true;
        }

        public FileExistenceInfo FileExists(string path)
        {
            FileSystemItem item;
            if (!_fileStructure.TryGetItem(path, out item) || item.Type != ItemType.File) return false;
            return item.Size;
        }

        public bool FolderExists(string path)
        {
            FileSystemItem item;
            return _fileStructure.TryGetItem(path, out item) && item.Type == ItemType.Directory;
        }

        public void DeleteFolder(string path)
        {
            throw new NotSupportedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotSupportedException();
        }

        public void CreateFolder(string path)
        {
            throw new NotSupportedException();
        }

        public byte[] ReadFileContent(string path)
        {
            var stream = _archive.Entries.First(e => e.FilePath == path).OpenEntryStream();
            return stream.ReadBytes((int)stream.Length);
        }

        public byte[] ReadFileContent(string path, bool saveTempFile, long fileSize)
        {
            return ReadFileContent(path);
        }

        public byte[] ReadFileHeader(string path)
        {
            var stream = _archive.Entries.First(e => e.FilePath == path).OpenEntryStream();
            return stream.ReadBytes(0x971A);
        }

        public void ExtractFile(string path, FileStream fs)
        {
            var entry = _archive.Entries.First(e => e.FilePath == path);
            var size = entry.Size;
            long totalBytesTransferred = 0;
            var percentage = 0;
            using (new Timer(s =>
                                 {
                                     long streamLength;
                                     try
                                     {
                                         streamLength = fs.Length;
                                     }
                                     catch
                                     {
                                         streamLength = size;
                                     }
                                     var transferred = streamLength - totalBytesTransferred;
                                     totalBytesTransferred = streamLength;
                                     percentage = (int) (totalBytesTransferred*100/size);
                                     var args = new TransferProgressChangedEventArgs(percentage, transferred, totalBytesTransferred, 0);
                                     _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(args);
                                 }, null, 100, 1))
            {
                entry.WriteTo(fs);
                while (percentage != 100)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public FileSystemItem Rename(string path, string newName)
        {
            throw new NotImplementedException();
        }

        public void Open(string path)
        {
            _archivePath = path;
            _archive = ArchiveFactory.Open(path);
            var isRar = _archive is RarArchive;
            _fileStructure = new Tree<FileSystemItem>();
            foreach (var entry in _archive.Entries.OrderBy(e => e.FilePath))
            {
                var entryPath = entry.FilePath;
                if (isRar) entryPath += BACKSLASH;
                _fileStructure.AddItem(entryPath, CreateModel(entry));
            }

            foreach (var key in _fileStructure.Keys.Where(key => key != string.Empty && !_fileStructure.ItemHasContent(key)))
            {
                _fileStructure.UpdateItem(key, CreateModel(key, true, DateTime.MinValue, null));
            }
        }

        public void Dispose()
        {
            _archive.Dispose();
            _archive = null;
            GC.Collect();
        }
    }
}