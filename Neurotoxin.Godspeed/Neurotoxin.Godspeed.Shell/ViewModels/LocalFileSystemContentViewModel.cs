using System;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LocalFileSystemContentViewModel : FileListPaneViewModelBase<LocalFileSystemContent>
    {
        private const string FREESPACE = "FreeSpace";
        private string _freeSpace;
        public string FreeSpace
        {
            get { return _freeSpace; }
            set { _freeSpace = value; NotifyPropertyChanged(FREESPACE); }
        }

        public LocalFileSystemContentViewModel(FileManagerViewModel parent) : base(parent)
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
                    Drive = Drives.First();
                    break;
            }
        }

        protected override void ChangeDrive()
        {
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == Drive.Path);
            DriveLabel = string.Format("[{0}]", string.IsNullOrEmpty(driveInfo.VolumeLabel) ? "_NONE_" : driveInfo.VolumeLabel);
            FreeSpace = String.Format("{0:#,0} of {1:#,0} bytes free", driveInfo.AvailableFreeSpace, driveInfo.TotalSize);
            base.ChangeDrive();
        }
    }
}