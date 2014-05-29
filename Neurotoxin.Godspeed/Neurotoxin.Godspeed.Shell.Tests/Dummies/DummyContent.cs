using System;
using System.Collections.Generic;
using System.Diagnostics;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;
using Microsoft.Practices.ObjectBuilder2;

namespace Neurotoxin.Godspeed.Shell.Tests.Dummies
{
    public class DummyContent : IFileManager
    {
        private Tree<FileSystemItem> _tree;
        public string TempFilePath { get; private set; }
        public char Slash { get; private set; }

        private FakingRules _fakingRules;
        public FakingRules FakingRules
        {
            get { return _fakingRules; }
            set
            {
                _fakingRules = value;
                if (_fakingRules.ItemTypes == null) _fakingRules.ItemTypes = new[] { ItemType.Directory, ItemType.File };
                if (_fakingRules.ItemTypesOnLevel == null)
                    _fakingRules.ItemTypesOnLevel = new Dictionary<int, ItemType[]>
                                                        {
                                                            {0, new[] {ItemType.Drive}}
                                                        };
                _tree = new Tree<FileSystemItem>(string.Empty, new FileSystemItem { Name = string.Empty, Path = string.Empty });
                var sw = new Stopwatch();
                sw.Start();
                GenerateTree(_tree, C.Random<int>(_fakingRules.TreeDepth));
                Console.WriteLine("[DDG] Generation completed in " + sw.Elapsed);
            }
        }

        private void GenerateTree(TreeItem<FileSystemItem> node, int depth, int level = 0)
        {
            var itemCount = C.Random<int>(_fakingRules.GetItemCount(level));
            var nodes = node.AddRange(C.CollectionOfFake<FileSystemItem>(itemCount, new { Type = _fakingRules.GetItemTypes(level), Name = new Range(8,13) }));
            foreach (var n in nodes)
            {
                n.Content.FullPath = n.Content.Path = node.Content.Path + "/" + n.Content.Name;
                if (level < depth) GenerateTree(n, depth, level+1);
            }
        }

        public IList<FileSystemItem> GetDrives()
        {
            return _tree.GetChildren("/");
        }

        public IList<FileSystemItem> GetList(string path = null)
        {
            if (path == null) throw new ArgumentNullException("path");
            return _tree.GetChildren(path);
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
            return true;
        }

        public FileExistenceInfo FileExists(string path)
        {
            var fake = _tree.Find(path);
            return fake != null && fake.Type == ItemType.File;
        }

        public bool FolderExists(string path)
        {
            var fake = _tree.Find(path);
            return fake != null && fake.Type == ItemType.Directory;
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
            _tree.Insert(path);
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
            var fake = _tree.Find(path);
            fake.Name = newName;
            return fake;
        }

        public void AddFile(FileSystemItem item)
        {
            _tree.Insert(item.Path, item);
        }
    }
}