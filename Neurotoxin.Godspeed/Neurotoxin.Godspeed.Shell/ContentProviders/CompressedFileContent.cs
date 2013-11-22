using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using SharpCompress.Archive;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class CompressedFileContent : IFileManager, IDisposable
    {
        private string _archivePath;
        private IArchive _archive;

        public string TempFilePath { get; set; }

        private const char SLASH = '/';
        public char Slash
        {
            get { return SLASH; }
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
            if (path == null) throw new NotSupportedException();
            var childrenPattern = new Regex(string.Format(@"^{0}[^/]+/?$", path));
            var entries = _archive.Entries.Where(e => childrenPattern.IsMatch(e.FilePath));
            return entries.Select(CreateModel).ToList();
        }

        public FileSystemItem GetFolderInfo(string path)
        {
            return GetFolderInfo(path, ItemType.Drive);
        }

        public FileSystemItem GetFolderInfo(string path, ItemType type)
        {
            return CreateModel(_archive.Entries.First(e => e.FilePath == path));
        }

        public FileSystemItem GetFileInfo(string path)
        {
            return CreateModel(_archive.Entries.First(e => e.FilePath == path));
        }

        private FileSystemItem CreateModel(IArchiveEntry entry)
        {
            var name = entry.FilePath.TrimEnd(SLASH);
            var slashIndex = name.LastIndexOf(SLASH);
            if (slashIndex > -1) name = name.Substring(slashIndex + 1);
            return new FileSystemItem
            {
                Name = name,
                Type = entry.IsDirectory ? ItemType.Directory : ItemType.File,
                Path = entry.FilePath,
                FullPath = string.Format(@"{0}:\{1}", _archivePath, entry.FilePath),
                Date = entry.LastModifiedTime ?? entry.LastAccessedTime ?? entry.CreatedTime ?? entry.ArchivedTime ?? DateTime.MinValue,
                Size = entry.IsDirectory ? (long?)null : entry.Size
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

        public bool FileExists(string path)
        {
            return _archive.Entries.Any(e => e.FilePath == path && !e.IsDirectory);
        }

        public bool FolderExists(string path)
        {
            return _archive.Entries.Any(e => e.FilePath == path && e.IsDirectory);
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
            _archive.Entries.First(e => e.FilePath == path).WriteTo(fs);
        }

        public void Open(string path)
        {
            _archivePath = path;
            _archive = ArchiveFactory.Open(path);
        }

        public void Dispose()
        {
            _archive = null;
            GC.Collect();
        }
    }
}