using System;
using System.Collections.Generic;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IFileManager
    {
        string TempFilePath { get; }
        char Slash { get; }

        IList<FileSystemItem> GetDrives();
        IList<FileSystemItem> GetList(string path = null);

        FileSystemItem GetItemInfo(string itemPath);
        FileSystemItem GetItemInfo(string itemPath, ItemType? type);
        FileSystemItem GetItemInfo(string itemPath, ItemType? type, bool swallowException);

        //FileSystemItem GetFolderInfo(string itemPath);
        //FileSystemItem GetFolderInfo(string itemPath, ItemType type);
        //FileSystemItem GetFileInfo(string itemPath);
        DateTime GetFileModificationTime(string path);

        bool DriveIsReady(string drive);
        FileExistenceInfo FileExists(string path);
        bool FolderExists(string path);

        void DeleteFolder(string path);
        void DeleteFile(string path);

        void CreateFolder(string path);

        byte[] ReadFileContent(string path, bool saveToTempFile, long fileSize);
        byte[] ReadFileHeader(string path);

        FileSystemItem Rename(string path, string newName);
    }
}