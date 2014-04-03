using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class LocalFileSystemContent : IFileManager
    {
        private readonly IEventAggregator _eventAggregator;
        private bool _isAborted;

        private const char SLASH = '\\';
        public char Slash
        {
            get { return SLASH; }
        }

        public string TempFilePath { get; set; }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int WNetGetConnection(
            [MarshalAs(UnmanagedType.LPTStr)] string localName,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
            ref int length);

        public LocalFileSystemContent(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public List<FileSystemItem> GetDrives()
        {
            var drives = DriveInfo.GetDrives();
            var result = new List<FileSystemItem>();
            foreach (var drive in drives)
            {
                var name = drive.Name.TrimEnd('\\');
                string icon;
                string fullPath = null;
                switch (drive.DriveType)
                {
                    case DriveType.CDRom:
                        icon = "drive_cd";
                        break;
                    case DriveType.Network:
                        icon = "drive_network";
                        var sb = new StringBuilder(512);
                        var size = sb.Capacity;
                        if (WNetGetConnection(name, sb, ref size) == 0) fullPath = sb.ToString().TrimEnd();
                        break;
                    case DriveType.Removable:
                        icon = "drive_flash";
                        if (!drive.IsReady) continue;
                        break;
                    default:
                        icon = "drive";
                        break;
                }
                var item = new FileSystemItem
                {
                    Path = drive.Name,
                    FullPath = fullPath ?? drive.Name,
                    Name = name,
                    Type = ItemType.Drive,
                    Thumbnail = ApplicationExtensions.GetContentByteArray(string.Format("/Resources/{0}.png", icon))
                };
                result.Add(item);
            }
            return result;
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();
            var list = Directory.GetDirectories(path).Select(p => GetDirectoryInfo(p)).ToList();
            list.AddRange(Directory.GetFiles(path).Select(GetFileInfo));
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
            if (path.EndsWith("\\") && Directory.Exists(path)) return GetDirectoryInfo(path, type);
            if (File.Exists(path)) return GetFileInfo(path);
            path += SLASH;
            return Directory.Exists(path) ? GetDirectoryInfo(path, type) : null;
        }

        private static FileSystemItem GetDirectoryInfo(string path, ItemType? type = null)
        {
            if (!path.EndsWith("\\")) path += SLASH;
            var dirInfo = new FileInfo(path);
            return new FileSystemItem
            {
                Name = Path.GetFileName(path.TrimEnd(SLASH)),
                Type = type ?? ItemType.Directory,
                Date = dirInfo.LastWriteTime,
                Path = path,
                FullPath = path,
                IsLink = dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)
            };
        }

        private static FileSystemItem GetFileInfo(string path)
        {
            var fileInfo = new FileInfo(path);
            return new FileSystemItem
            {
                Name = fileInfo.Name,
                Type = ItemType.File,
                Date = fileInfo.LastWriteTime,
                Path = path,
                FullPath = path,
                Size = fileInfo.Length,
            };
        }

        public DateTime GetFileModificationTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public bool DriveIsReady(string drive)
        {
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == drive);
            return driveInfo != null && driveInfo.IsReady;
        }

        public FileExistenceInfo FileExists(string path)
        {
            var file = new FileInfo(path);
            if (file.Exists) return file.Length;
            return false;
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public void DeleteFolder(string path)
        {
            Directory.Delete(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public byte[] ReadFileContent(string path, bool saveTempFile, long fileSize)
        {
            return File.ReadAllBytes(path);
        }

        public byte[] ReadFileHeader(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var bytes = new byte[0x971A];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            return bytes;
        }

        public void CopyFile(string path, FileStream fs, long resumeStartPosition)
        {
            _isAborted = false;
            var totalBytesTransferred = resumeStartPosition;
            var readStream = File.Open(path, FileMode.Open, FileAccess.Read);
            var totalBytes = readStream.Length;
            readStream.Seek(resumeStartPosition, SeekOrigin.Begin);
            var buffer = new byte[32768];
            int bytesRead;
            _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(-1, resumeStartPosition, resumeStartPosition, resumeStartPosition));
            while ((bytesRead = readStream.Read(buffer, 0, buffer.Length)) > 0 && !_isAborted)
            {
                fs.Write(buffer, 0, bytesRead);
                totalBytesTransferred += bytesRead;
                var percentage = (int)(totalBytesTransferred / totalBytes * 100);
                _eventAggregator.GetEvent<TransferProgressChangedEvent>().Publish(new TransferProgressChangedEventArgs(percentage, bytesRead, totalBytesTransferred, resumeStartPosition));
            }
            readStream.Close();
        }

        public void AbortCopy()
        {
            _isAborted = true;
        }

        public FileSystemItem Rename(string path, string newName)
        {
            var oldName = Path.GetFileName(path.TrimEnd('\\'));
            var r = new Regex(string.Format(@"{0}\\?$", Regex.Escape(oldName)), RegexOptions.IgnoreCase);
            var newPath = r.Replace(path, newName); 
            
            if (FolderExists(path))
            {
                try
                {
                    Directory.Move(path, newPath + Slash);
                    return GetDirectoryInfo(newPath);
                }
                catch (Exception ex)
                {
                    NotificationMessage.ShowMessage(Resx.IOError, ex.Message);
                    return GetDirectoryInfo(path);
                }
            }
            else
            {
                try
                {
                    File.Move(path, newPath);
                    return GetFileInfo(newPath);
                }
                catch (Exception ex)
                {
                    NotificationMessage.ShowMessage(Resx.IOError, ex.Message);
                    return GetFileInfo(path);
                }
            }
        }
    }
}