using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class StfsPackageContentViewModel : FileListPaneViewModelBase<StfsPackageContent>
    {

        #region SaveAndCloseCommand

        public DelegateCommand SaveAndCloseCommand { get; private set; }

        private void ExecuteSaveAndCloseCommand()
        {
            var bytes = FileManager.Save();
            FileManager.Dispose();
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, bytes));
        }

        #endregion

        #region CloseCommand

        public DelegateCommand CloseCommand { get; private set; }

        private void ExecuteCloseCommand()
        {
            FileManager.Dispose();
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
        }

        #endregion

        public StfsPackageContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            SaveAndCloseCommand = new DelegateCommand(ExecuteSaveAndCloseCommand);
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(
                        () =>
                            {
                                FileManager.LoadPackage((byte[])cmdParam);
                                return true;
                            },
                        result =>
                            {
                                Drives = FileManager.GetDrives().Select(d => new FileSystemItemViewModel(d)).ToObservableCollection();
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

        protected override void SaveToFileStream(string path, FileStream fs, long remoteStartPosition)
        {
            FileManager.ExtractFile(path, fs, remoteStartPosition);
        }

        protected override void CreateFile(string targetPath, string sourcePath)
        {
            FileManager.AddFile(targetPath, sourcePath);
        }

        protected override void OverwriteFile(string targetPath, string sourcePath)
        {
            FileManager.ReplaceFile(targetPath, sourcePath);
        }

        protected override void ResumeFile(string targetPath, string sourcePath)
        {
            FileManager.ReplaceFile(targetPath, sourcePath);
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}