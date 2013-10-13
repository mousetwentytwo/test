using System;
using System.IO;
using System.Linq;
using Neurotoxin.Contour.Modules.FileManager.ContentProviders;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public class StfsPackageContentViewModel : FileListPaneViewModelBase<StfsPackageContent>
    {
        public StfsPackageContentViewModel(FileManagerViewModel parent, byte[] content) : base(parent, new StfsPackageContent(content))
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action success = null, Action error = null)
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
            DriveLabel = FileManager.GetAccount().GamerTag;
            base.ChangeDrive();
        }
    }
}