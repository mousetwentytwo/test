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

        #region CloseCommand

        private void ExecuteCloseCommand()
        {
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
        }

        #endregion

        public CompressedFileContentViewModel()
        {
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

        protected override bool SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            FileManager.ExtractFile(item.Path, fs);
            return true;
        }

        protected override bool CreateFile(string targetPath, FileSystemItem source)
        {
            throw new NotImplementedException();
        }

        protected override bool OverwriteFile(string targetPath, FileSystemItem source)
        {
            throw new NotImplementedException();
        }

        protected override bool ResumeFile(string targetPath, FileSystemItem source)
        {
            throw new NotImplementedException();
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