using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
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
            return DriveInfo.GetDrives().Select(drive =>
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
                    return item;
                }).ToList();
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();

            var list = Directory.GetDirectories(path).Select(GetFolderInfo).ToList();
            list.AddRange(Directory.GetFiles(path).Select(GetFileInfo));

            return list;
        }

        public FileSystemItem GetFolderInfo(string path)
        {
            return GetFolderInfo(path, ItemType.Directory);
        }

        public FileSystemItem GetFolderInfo(string path, ItemType type)
        {
            var p = path.EndsWith("\\") ? path : path + "\\";
            return new FileSystemItem
            {
                Name = Path.GetFileName(path),
                Type = type,
                Date = Directory.GetLastWriteTime(path),
                Path = p,
                FullPath = p
            };
        }

        public FileSystemItem GetFileInfo(string path)
        {
            return new FileSystemItem
            {
                Name = Path.GetFileName(path),
                Type = ItemType.File,
                Date = File.GetLastWriteTime(path),
                Path = path,
                FullPath = path,
                Size = new FileInfo(path).Length,
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
    }
}