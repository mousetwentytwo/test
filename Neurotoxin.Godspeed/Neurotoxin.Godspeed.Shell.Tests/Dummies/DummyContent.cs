using System;
using System.Collections.Generic;
using FakeItEasy;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class DummyContent : IFileManager
    {
        public string TempFilePath { get; private set; }
        public char Slash { get; private set; }

        public IList<FileSystemItem> GetDrives()
        {
            var itemCount = C.Random<int>(1,5);
            return C.CollectionOfFake<FileSystemItem>(itemCount);
        }

        public IList<FileSystemItem> GetList(string path = null)
        {
            var itemCount = C.Random<int>(0, 100);
            return C.CollectionOfFake<FileSystemItem>(itemCount);
        }

        public FileSystemItem GetItemInfo(string itemPath)
        {
            var fake = C.Fake<FileSystemItem>();
            fake.Path = itemPath;
            return fake;
        }

        public FileSystemItem GetItemInfo(string itemPath, ItemType? type)
        {
            var fake = GetItemInfo(itemPath);
            if (type != null) fake.Type = type.Value;
            return fake;
        }

        public FileSystemItem GetItemInfo(string itemPath, ItemType? type, bool swallowException)
        {
            //TODO: implement swallowException handling
            return GetItemInfo(itemPath, type);
        }

        public DateTime GetFileModificationTime(string path)
        {
            return C.Random<DateTime>(-7*24*3600, -1000);
        }

        public bool DriveIsReady(string drive)
        {
            //TODO
            return true;
        }

        public FileExistenceInfo FileExists(string path)
        {
            //TODO
            return true;
        }

        public bool FolderExists(string path)
        {
            //TODO
            return true;
        }

        public void DeleteFolder(string path)
        {
            //TODO
        }

        public void DeleteFile(string path)
        {
            //TODO
        }

        public void CreateFolder(string path)
        {
            //TODO
        }

        public byte[] ReadFileContent(string path, bool saveToTempFile, long fileSize)
        {
            return C.Random<byte[]>(0x971A, 0xFFFF);
        }

        public byte[] ReadFileHeader(string path)
        {
            return C.Random<byte[]>(0x971A, 0x971A);
        }

        public FileSystemItem Rename(string path, string newName)
        {
            var fake = C.Fake<FileSystemItem>();
            //TODO: fake.Path
            fake.Name = newName;
            return fake;
        }
    }
}