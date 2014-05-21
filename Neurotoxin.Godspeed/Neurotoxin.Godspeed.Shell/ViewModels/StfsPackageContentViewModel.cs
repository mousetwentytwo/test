using System;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StfsPackageContentViewModel : FileListPaneViewModelBase<StfsPackageContent>
    {
        private BinaryContent _packageContent;

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected override string ExportActionDescription
        {
            get { return Resx.Extract; }
        }

        protected override string ImportActionDescription
        {
            get { return Resx.Inject; }
        }

        public override bool IsVerificationEnabled
        {
            get { return false; }
        }

        #region SaveAndCloseCommand

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        private void ExecuteSaveAndCloseCommand()
        {
            _packageContent.Content = FileManager.Save();
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, _packageContent));
            Dispose();
        }

        #endregion

        #region CloseCommand

        private void ExecuteCloseCommand()
        {
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
            Dispose();
        }

        #endregion

        public StfsPackageContentViewModel()
        {
            SaveAndCloseCommand = new DelegateCommand(ExecuteSaveAndCloseCommand);
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            IsResumeSupported = true;
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(
                        () =>
                            {
                                _packageContent = (BinaryContent)cmdParam.Payload;
                                FileManager.LoadPackage(_packageContent);
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

        protected override void ChangeDrive()
        {
            DriveLabel = FileManager.GetAccount().GamerTag;
            base.ChangeDrive();
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        protected override bool SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            FileManager.ExtractFile(item.Path, fs, remoteStartPosition);
            return true;
        }

        protected override bool CreateFile(string targetPath, FileSystemItem source)
        {
            FileManager.AddFile(targetPath, source.Path);
            return true;
        }

        protected override bool OverwriteFile(string targetPath, FileSystemItem source)
        {
            FileManager.ReplaceFile(targetPath, source.Path);
            return true;
        }

        protected override bool ResumeFile(string targetPath, FileSystemItem source)
        {
            FileManager.ReplaceFile(targetPath, source.Path);
            return true;
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            FileManager.Dispose();
            base.Dispose();
        }
    }
}