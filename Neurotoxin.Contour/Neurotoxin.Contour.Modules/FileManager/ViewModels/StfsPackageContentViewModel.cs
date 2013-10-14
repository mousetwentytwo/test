using System;
using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Neurotoxin.Contour.Modules.FileManager.ContentProviders;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public class StfsPackageContentViewModel : FileListPaneViewModelBase<StfsPackageContent>
    {
        public StfsPackageContentViewModel(FileManagerViewModel parent, IUnityContainer container) : base(parent, container)
        {
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    FileManager.LoadPackage((byte[])cmdParam);
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