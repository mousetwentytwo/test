using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.ContentProviders
{
    public class LocalFileSystemContent : IFileManager
    {
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

            var di = new System.IO.DirectoryInfo(path);
            if (di.Attributes.HasFlag(FileAttributes.ReparsePoint))
                path = new ReparsePoint(path).Target;

            var list = Directory.GetDirectories(path).Select(GetDirectoryInfo).ToList();
            list.AddRange(Directory.GetFiles(path).Select(GetFileInfo));
            return list;
        }

        public FileSystemItem GetItemInfo(string path)
        {
            return GetItemInfo(path, null);
        }

        public FileSystemItem GetItemInfo(string path, ItemType? type)
        {
            if (path.EndsWith("\\") && Directory.Exists(path)) return GetDirectoryInfo(path);
            if (File.Exists(path)) return GetFileInfo(path);
            path += SLASH;
            return Directory.Exists(path) ? GetDirectoryInfo(path) : null;
        }

        private static FileSystemItem GetDirectoryInfo(string path)
        {
            if (!path.EndsWith("\\")) path += SLASH;
            return new FileSystemItem
                       {
                           Name = Path.GetFileName(path.TrimEnd(SLASH)),
                           Type = ItemType.Directory,
                           Date = Directory.GetLastWriteTime(path),
                           Path = path,
                           FullPath = path
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
            try
            {
                return File.GetLastWriteTime(path);
            }
            catch (IOException ex)
            {
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
        }

        public bool DriveIsReady(string drive)
        {
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == drive);
            return driveInfo.IsReady;
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public void DeleteFolder(string path)
        {
            try
            {
                Directory.Delete(path);
            }
            catch (IOException ex)
            {
                //TODO: not read
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
        }

        public void DeleteFile(string path)
        {
            try {
                File.Delete(path);
            }
            catch (IOException ex)
            {
                //TODO: not read
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
        }

        public void CreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (IOException ex)
            {
                //TODO: not read
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
        }

        public byte[] ReadFileContent(string path, bool saveTempFile, long fileSize)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (IOException ex)
            {
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
        }

        public byte[] ReadFileHeader(string path)
        {
            try
            {
                var fs = new FileStream(path, FileMode.Open);
                var bytes = new byte[0x971A];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                return bytes;
            }
            catch (Exception ex)
            {
                throw new TransferException(TransferErrorType.ReadAccessError, ex.Message);
            }
        }

        public FileSystemItem Rename(string path, string newName)
        {
            var oldName = Path.GetFileName(path.TrimEnd('\\'));
            var r = new Regex(string.Format(@"{0}\\?$", Regex.Escape(oldName)), RegexOptions.IgnoreCase);
            var newPath = r.Replace(path, newName); 
            
            if (FolderExists(path))
            {
                Directory.Move(path, newPath + Slash);
                return GetDirectoryInfo(newPath);
            }
            else
            {
                File.Move(path, newPath);
                return GetFileInfo(newPath);
            }
        }
    }
}