using System;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StfsPackageContentViewModel : FileListPaneViewModelBase<StfsPackageContent>, IDisposablePane
    {

        #region CloseCommand

        public string CloseButtonText { get; private set; }

        public DelegateCommand<EventInformation<EventArgs>> CloseCommand { get; private set; }

        private void ExecuteCloseCommand(EventInformation<EventArgs> cmdParam)
        {
            FileManager.Save();
            //TODO: via event aggregator
            //UNDONE: stack panes, dispose this, and pop previous
            Parent.FtpDisconnect();
        }

        #endregion

        public StfsPackageContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            CloseButtonText = "Save & Close";
            CloseCommand = new DelegateCommand<EventInformation<EventArgs>>(ExecuteCloseCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
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