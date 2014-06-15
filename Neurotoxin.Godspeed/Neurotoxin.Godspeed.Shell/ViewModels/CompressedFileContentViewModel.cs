using System;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class CompressedFileContentViewModel : FileListPaneViewModelBase<CompressedFileContent>
    {
        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override bool IsFSD
        {
            get { return false; }
        }

        public override bool IsVerificationEnabled
        {
            get { return false; }
        }

        #region CloseCommand

        private void ExecuteCloseCommand()
        {
            EventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
        }

        #endregion

        public CompressedFileContentViewModel()
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkHandler.Run(
                        () =>
                            {
                                FileManager.Open((string)cmdParam.Payload);
                                return true;
                            },
                        result =>
                            {
                                IsLoaded = true;
                                Initialize();
                                Drive = Drives.First();
                                if (success != null) success.Invoke(this);
                            },
                        exception =>
                            {
                                if (error != null) error.Invoke(this, exception);
                            });
                    break;
            }
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        public override void Dispose()
        {
            FileManager.Dispose();
            base.Dispose();
        }
    }
}