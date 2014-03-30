using System.Collections.Generic;
using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Io.Stfs.Data;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FileEntryViewModel : ViewModelBase
    {
        private FileEntry _model;

        public string Name
        {
            get { return _model.Name; }
        }

        public bool IsDirectory
        {
            get { return _model.IsDirectory; }
        }

        public int FileSize
        {
            get { return _model.FileSize; }
        }

        private readonly ObservableCollection<FileBlockViewModel> _blocks;
        public ObservableCollection<FileBlockViewModel> Blocks
        {
            get { return _blocks; }
        }

        public FileEntryViewModel(FileEntry model, IEnumerable<KeyValuePair<int?, BlockStatus>> blockList)
        {
            _model = model;
            _blocks = new ObservableCollection<FileBlockViewModel>();
            foreach (var block in blockList)
            {
                FileBlockHealthStatus status;
                if (!block.Key.HasValue) status = FileBlockHealthStatus.Missing;
                else
                {
                    switch (block.Value)
                    {
                        case BlockStatus.Allocated:
                        case BlockStatus.NewlyAllocated:
                            status = FileBlockHealthStatus.Ok;
                            break;
                        default:
                            status = FileBlockHealthStatus.Unallocated;
                            break;
                    }
                }
                var vm = new FileBlockViewModel(block.Key, status);
                _blocks.Add(vm);
            }
        }
    }
}