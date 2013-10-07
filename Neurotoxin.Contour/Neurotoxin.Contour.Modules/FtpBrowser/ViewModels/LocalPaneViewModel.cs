using System;
using System.IO;
using System.Linq;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class LocalPaneViewModel : PaneViewModelBase<LocalWrapper>
    {
        public LocalPaneViewModel(ModuleViewModelBase parent) : base(parent, new LocalWrapper())
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
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