using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Helpers;
using Neurotoxin.Godspeed.Core.Io.Stfs;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class ProfileRebuilderViewModel : PaneViewModelBase
    {
        private BinaryContent _packageContent;
        private StfsPackage _stfs;

        #region Properties

        private const string FILEENTRYVIEWMODEL = "FileEntryViewModel";
        public ObservableCollection<FileEntryViewModel> _fileStructure;
        public ObservableCollection<FileEntryViewModel> FileStructure
        {
            get { return _fileStructure; }
            set { _fileStructure = value; NotifyPropertyChanged(FILEENTRYVIEWMODEL); }
        }

        #endregion

        #region CloseCommand

        public DelegateCommand CloseCommand { get; private set; }

        private void ExecuteCloseCommand()
        {
        }

        #endregion

        public ProfileRebuilderViewModel(FileManagerViewModel parent) : base(parent)
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(
                        () =>
                        {
                            _packageContent = (BinaryContent)cmdParam;
                            _stfs = ModelFactory.GetModel<StfsPackage>(_packageContent.Content);
                            var c = _stfs.VolumeDescriptor.AllocatedBlockCount;
                            var d = new Dictionary<int, int?>();
                            //_stfs.SwitchToBackupTables();

                            for (var i = 0; i < c; i++)
                            {
                                var offset = _stfs.GetRealAddressOfBlock(i);
                                var bytes = _stfs.Binary.ReadBytes(offset, 4);
                                if (Encoding.Default.GetString(bytes) == "XDBF") Debug.WriteLine(i);
                            }

                            var y = 0;

                            for (var i = 0; i < _stfs.TopTable.EntryCount; i++)
                            {
                                var tableEntry = _stfs.TopTable.Entries[i];
                                //var address = _stfs.GetHashTableAddress(i, 0);
                                //var b1 = _stfs.Binary.ReadBytes(address, 0x1000);
                                //var b2 = _stfs.Binary.ReadBytes(address + 0x1000, 0x1000);
                                //for (var k = 0; k < 170; k++)
                                //{
                                //    Debug.WriteLine("HashEntry #{0}", i*170 + k);
                                //    var h1 = ModelFactory.GetModel<HashEntry>(b1, k * HashEntry.Size);
                                //    var h2 = ModelFactory.GetModel<HashEntry>(b2, k * HashEntry.Size);
                                //    BinMapHelper.ModelCompare(h1, h2);
                                //}

                                //var sb = new StringBuilder();
                                //var db = 0;
                                //for (var k = 0; k < 0x1000; k++)
                                //{
                                //    if (b1[k] != b2[k])
                                //    {
                                //        db++;
                                //        if (sb.Length > 0) sb.Append(", ");
                                //        sb.Append(k);
                                //    }
                                //}
                                //Debug.WriteLine("[{0}][{1}] {2}", i, db, sb);

                                var table = _stfs.TopTable.Tables[i];
                                for (var j = 0; j < 170; j++)
                                {
                                    var entry = table.Entries[j];
                                    d.Add(entry.Block.Value, entry.NextBlock <= c ? entry.NextBlock : (int?)null);
                                    //Debug.WriteLine("[{0,3}][{1}] -> {2}", entry.Block, entry.Status.ToString()[0], entry.NextBlock <= c ? entry.NextBlock.ToString() : "*");
                                }
                                //var u = table.Entries.Count(e => e.Status == BlockStatus.Unallocated);
                                //var p = table.Entries.Count(e => e.Status == BlockStatus.PreviouslyAllocated);

                                //var tp = tableEntry.NextBlock >> 15;
                                //var tu = tableEntry.NextBlock & ~(tp << 15);
                                //Debug.WriteLine("{0} {1} vs {2} {3}", u, p, tu, tp);
                            }

                            var ends = d.Where(kvp => (kvp.Value == null || kvp.Value.Value == 0) && (kvp.Key != 0));
                            foreach (var end in ends)
                            {
                                var s = new List<Stack<int>>();
                                s.Add(new Stack<int>());
                                var key = new List<int?> { end.Key };
                                do
                                {
                                    for (var i = 0; i < key.Count; i++)
                                    {
                                        if (s.Count <= i) s.Add(new Stack<int>(s[i-1].Reverse()));
                                        s[i].Push(key[i].Value);
                                    }
                                    key = d.Where(kvp => key.Contains(kvp.Value)).Select(kvp => (int?)kvp.Key).ToList();
                                } while (key.Count != 0);
                                foreach (var ss in s)
                                {
                                    var sb = new StringBuilder();
                                    do
                                    {
                                        if (sb.Length != 0) sb.Append(", ");
                                        sb.Append(ss.Pop());
                                    }
                                    while (ss.Count != 0);
                                    Debug.WriteLine(sb);
                                }
                            }

                            var x = 0;

                            //for (var i = 0; i<_stfs.TopTable.EntryCount; i++)
                            //{
                            //    var tableEntry = _stfs.TopTable.Entries[i];
                            //    var table = _stfs.TopTable.Tables[i];
                            //    var u = table.Entries.Count(e => e.Status == BlockStatus.Unallocated);
                            //    var p = table.Entries.Count(e => e.Status == BlockStatus.PreviouslyAllocated);

                            //    var tp = tableEntry.NextBlock >> 15;
                            //    var tu = tableEntry.NextBlock & ~(tp << 15);
                            //    Debug.WriteLine("{0} {1} vs {2} {3}", u, p, tu, tp);
                            //}

                            //var x = _stfs.TopTable.Entries.Select(e => e.Status).ToList();
                            
                            return true;
                        },
                        result =>
                            {
                                ParseStfs();
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                        {
                            if (error != null) error.Invoke(this, exception);
                        });
                    break;
            }
        }

        private List<FileEntry> ParseFileList(byte[] block)
        {
            var f = new List<FileEntry>();
            for (var i = 0; i < 64; i++)
            {
                var addr = i * 0x40;
                var fe = ModelFactory.GetModel<FileEntry>(block, addr);
                fe.EntryIndex = i;

                if (fe.Name != String.Empty)
                    f.Add(fe);
            }
            return f;
        }

        private void ParseStfs()
        {
            //var block0 = _stfs.ExtractBlock(0);
            //var f0 = ParseFileList(block0);
            //var block1 = _stfs.ExtractBlock(1);
            //var f1 = ParseFileList(block1);

            //for (var i = 0; i < f0.Count; i++)
            //{
            //    Debug.WriteLine(f0[i].Name);
            //    BinMapHelper.ModelCompare(f0[i], f1[i]);
            //}

            var collection = new ObservableCollection<FileEntryViewModel>();
            var allocatedBlocks = new HashSet<int>();
            var blockCollisions = new HashSet<int>();
            foreach (var fileEntry in _stfs.FlatFileList.Where(f => !f.IsDirectory).OrderBy(f => f.Name))
            {
                var blockList = _stfs.GetFileEntryBlockList(fileEntry);
                foreach (var block in blockList.Where(b => b.Key.HasValue))
                {
                    if (!allocatedBlocks.Contains(block.Key.Value))
                        allocatedBlocks.Add(block.Key.Value);
                    else
                        blockCollisions.Add(block.Key.Value);
                }
                collection.Add(new FileEntryViewModel(fileEntry, blockList));
            }

            foreach (var block in blockCollisions.SelectMany(blockCollision => collection.SelectMany(vm => vm.Blocks.Where(b => b.BlockNumber == blockCollision))))
            {
                block.Health = FileBlockHealthStatus.Collision;
            }

            var ok = 0;
            var size = 0;
            var allSize = 0;
            foreach (var vm in collection)
            {
                allSize += vm.FileSize;
                if (vm.Blocks.All(b => b.Health == FileBlockHealthStatus.Ok || b.Health == FileBlockHealthStatus.Collision))
                {
                    ok++;
                    size += vm.FileSize;
                }
            }

            FileStructure = collection;
        }

    }
}