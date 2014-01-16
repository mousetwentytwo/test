using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                            //_stfs.SwitchToBackupTables();
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