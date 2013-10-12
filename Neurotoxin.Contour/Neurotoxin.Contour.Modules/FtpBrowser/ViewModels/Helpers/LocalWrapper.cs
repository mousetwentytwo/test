using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Exceptions;
using Neurotoxin.Contour.Modules.FtpBrowser.Interfaces;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers
{
    public class LocalWrapper : IFileManager
    {
        public List<FileSystemItem> GetDrives()
        {
            return DriveInfo.GetDrives().Select(drive => new FileSystemItem
            {
                Path = drive.Name,
                Name = drive.Name.TrimEnd('\\'),
                Type = ItemType.Drive
            }).ToList();
        }

        public List<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new NotSupportedException();

            var list = Directory.GetDirectories(path).Select(di => new FileSystemItem
                                                                       {
                                                                           Name = Path.GetFileName(di), 
                                                                           Type = ItemType.Directory, 
                                                                           Date = Directory.GetLastWriteTime(di), 
                                                                           Path = string.Format("{0}\\", di),
                                                                       }).ToList();
            list.AddRange(Directory.GetFiles(path).Select(fi => new FileSystemItem
                                                                    {
                                                                        Name = Path.GetFileName(fi), 
                                                                        Type = ItemType.File, 
                                                                        Date = File.GetLastWriteTime(fi), 
                                                                        Path = fi, 
                                                                        Size = new FileInfo(fi).Length,
                                                                    }));

            return list;
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
                Directory.Delete(path, true);
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

        public byte[] ReadFileContent(string path)
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