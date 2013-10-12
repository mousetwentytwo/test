using System;
using System.Collections.Generic;
using Neurotoxin.Contour.Modules.FileManager.Models;

namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface IFileManager
    {
        List<FileSystemItem> GetDrives(); 
        List<FileSystemItem> GetList(string path = null);

        FileSystemItem GetFileInfo(string itemPath);
        DateTime GetFileModificationTime(string path);

        bool DriveIsReady(string drive);
        bool FileExists(string path);
        bool FolderExists(string path);

        void DeleteFolder(string path);
        void DeleteFile(string path);

        void CreateFolder(string path);

        byte[] ReadFileContent(string path);
        byte[] ReadFileHeader(string path);
    }
}