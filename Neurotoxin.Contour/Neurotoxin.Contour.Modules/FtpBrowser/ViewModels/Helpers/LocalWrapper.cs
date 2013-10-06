using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            Directory.Delete(path, true);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public byte[] ReadFileContent(string path)
        {
            return File.ReadAllBytes(path);
        }

        public byte[] ReadFileHeader(string path)
        {
            var fs = new FileStream(path, FileMode.Open);
            var bytes = new byte[0x971A];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            return bytes;
        }
    }
}