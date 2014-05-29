using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Neurotoxin.Godspeed.Core.Io;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Exceptions;
using Neurotoxin.Godspeed.Shell.Models;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class LocalFileSystemContentViewModel : FileListPaneViewModelBase<LocalFileSystemContent>
    {
        public bool IsNetworkDrive
        {
            get { return Drive.FullPath.StartsWith(@"\\"); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected override string ExportActionDescription
        {
            get { return null; }
        }

        protected override string ImportActionDescription
        {
            get { return null; }
        }

        public override bool IsVerificationSupported
        {
            get { return false; }
        }

        public override bool IsVerificationEnabled
        {
            get { return false; }
        }

        public LocalFileSystemContentViewModel()
        {
            IsResumeSupported = true;
            EventAggregator.GetEvent<UsbDeviceChangedEvent>().Subscribe(OnUsbDeviceChanged);
        }

        public override void LoadDataAsync(LoadCommand cmd, LoadDataAsyncParameters cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null)
        {
            base.LoadDataAsync(cmd, cmdParam, success, error);
            switch (cmd)
            {
                case LoadCommand.Load:
                    Initialize();
                    var storedDrive = Path.GetPathRoot(Settings.Directory);
                    var drive = Drives.FirstOrDefault(d => d.Path == storedDrive);
                    if (drive != null) PathCache.Add(drive, Settings.Directory);
                    Drive = drive ?? GetDefaultDrive();
                    break;
                case LoadCommand.Restore:
                    var payload = cmdParam.Payload as BinaryContent;
                    if (payload == null) return;
                    WorkHandler.Run(
                        () =>
                        {
                            File.WriteAllBytes(payload.FilePath, payload.Content);
                            return true;
                        },
                        result =>
                        {
                            if (success != null) success.Invoke(this);
                        },
                        exception =>
                        {
                            if (error != null) error.Invoke(this, exception);
                        });
                    break;
            }
        }

        private FileSystemItemViewModel GetDefaultDrive()
        {
            if (Drives.Count == 0) return null;
            return Drives.FirstOrDefault(d => d.Name == "C:") ?? Drives.First();
        }


        protected override void ChangeDrive()
        {
            base.ChangeDrive();
            UpdateDriveInfo();
        }

        protected override void ChangeDirectory(string message = null, Action callback = null)
        {
            if (CurrentFolder.Type == ItemType.Link)
            {
                var reparsePoint = new ReparsePoint(CurrentFolder.Path);
                var path = reparsePoint.Target;
                if (path == null)
                {
                    if (reparsePoint.LastError == 5)
                    {
                        try
                        {
                            var cmd = string.Format("/k dir \"{0}\" /AL", Path.Combine(CurrentFolder.Path, ".."));
                            var p = Process.Start(new ProcessStartInfo("cmd.exe", cmd)
                            {
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false
                            });
                            var r = new Regex(string.Format("{0} \\[(.*?)\\]", Regex.Escape(CurrentFolder.Name)), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            var m = r.Match(p.StandardOutput.ReadToEnd());
                            path = m.Groups[1].Value;
                            p.Close();
                        }
                        catch
                        {
                            //do nothing if something goes wrong
                        }
                    }

                    if (string.IsNullOrEmpty(path))
                    {
                        WindowManager.ShowMessage(Resx.IOError, reparsePoint.LastError == 5 ? Resx.ReparsePointCannotBeAccessed : Resx.ReparsePointCannotBeResolved);
                        return;
                    }
                }
                var model = FileManager.GetItemInfo(path, ItemType.Directory);
                if (model == null)
                {
                    WindowManager.ShowMessage(Resx.IOError, string.Format(Resx.ItemNotExistsOnPath, path));
                    return;
                }
                CurrentFolder = new FileSystemItemViewModel(model);
            }
            base.ChangeDirectory(message, callback);
        }

        protected override void ChangeDirectoryCallback(IList<FileSystemItem> result)
        {
            base.ChangeDirectoryCallback(result);
            UpdateDriveInfo();
        }

        private void UpdateDriveInfo()
        {
            var driveInfo = DriveInfo.GetDrives().First(d => d.Name == Drive.Path);
            DriveLabel = string.Format("[{0}]", string.IsNullOrEmpty(driveInfo.VolumeLabel) ? "_NONE_" : driveInfo.VolumeLabel);
            FreeSpace = String.Format(Resx.LocalFileSystemFreeSpace, driveInfo.AvailableFreeSpace, driveInfo.TotalSize);
        }

        public override string GetTargetPath(string path)
        {
            return string.Format("{0}{1}", CurrentFolder.Path, path.Replace('/', '\\'));
        }

        protected override bool SaveToFileStream(FileSystemItem item, FileStream fs, long remoteStartPosition)
        {
            return FileManager.CopyFile(item.Path, fs, remoteStartPosition);
        }

        protected override Exception WrapTransferRelatedExceptions(Exception exception)
        {
            if (exception is IOException)
                return new TransferException(TransferErrorType.NotSpecified, exception.Message, exception);

            return base.WrapTransferRelatedExceptions(exception);
        }

        protected override bool CreateFile(string targetPath, FileSystemItem source)
        {
            //TODO: local2local
            throw new NotImplementedException();
        }

        protected override bool OverwriteFile(string targetPath, FileSystemItem source)
        {
            //TODO: local2local
            throw new NotImplementedException();
        }

        protected override bool ResumeFile(string targetPath, FileSystemItem source)
        {
            //TODO: local2local
            throw new NotImplementedException();
        }

        public override void Abort()
        {
            FileManager.AbortCopy();
        }

        protected override string GetStfsPackagePath(CacheComplexKey cacheKey, CacheEntry<FileSystemItem> cacheEntry)
        {
            return cacheKey.Item.Path;
        }


        private void OnUsbDeviceChanged(UsbDeviceChangedEventArgs e)
        {
            var drives = FileManager.GetDrives();
            for (var i = 0; i < drives.Count; i++)
            {
                var current = drives[i];
                if (i < Drives.Count && current.Name == Drives[i].Name) continue;
                if (i < Drives.Count && Drives.Any(d => d.Name == current.Name))
                {
                    var existing = Drives[i];
                    if (Drive.Name == existing.Name)
                        Drive = GetDefaultDrive();
                    Drives.Remove(existing);
                } 
                else
                {
                    Drives.Insert(i, new FileSystemItemViewModel(current));
                }
            }
        }

        public override void Dispose()
        {
            EventAggregator.GetEvent<UsbDeviceChangedEvent>().Unsubscribe(OnUsbDeviceChanged);
            base.Dispose();
        }
    }
}