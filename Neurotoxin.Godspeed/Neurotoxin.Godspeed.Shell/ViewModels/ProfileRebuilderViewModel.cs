using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neurotoxin.Godspeed.Core.Io.Stfs;
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
                            return true;
                        },
                        result =>
                            {
                                IsLoaded = true;
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

        private void ParseStfs()
        {
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

            FileStructure = collection;
        }

    }
}