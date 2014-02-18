using System;
using System.IO;
using System.Linq;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Models;

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
            get { return "Extract"; }
        }

        protected override string ImportActionDescription
        {
            get { return "Inject"; }
        }

        #region CloseCommand

        private void ExecuteCloseCommand()
        {
            eventAggregator.GetEvent<CloseNestedPaneEvent>().Publish(new CloseNestedPaneEventArgs(this, null));
        }

        #endregion

        public CompressedFileContentViewModel(FileManagerViewModel parent) : base(parent)
        {
            CloseCommand = new DelegateCommand(ExecuteCloseCommand);
            IsResumeSupported = true;
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    WorkerThread.Run(
                        () =>
                            {
                                var p = (Tuple<string, FileListPaneSettings>) cmdParam;
                                Settings = p.Item2;
                                FileManager.Open(p.Item1);
                                return true;
                            },
                        result =>
                            {
                                IsLoaded = true;
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

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('\\', '/'));
        }

        protected override void SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            FileManager.ExtractFile(item.Path, fs);
        }

        protected override void CreateFile(string targetPath, string sourcePath)
        {
            throw new NotImplementedException();
        }

        protected override void OverwriteFile(string targetPath, string sourcePath)
        {
            throw new NotImplementedException();
        }

        protected override void ResumeFile(string targetPath, string sourcePath)
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