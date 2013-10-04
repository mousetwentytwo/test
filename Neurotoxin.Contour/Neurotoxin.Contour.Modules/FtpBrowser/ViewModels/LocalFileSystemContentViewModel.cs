using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class LocalPaneViewModel : PaneViewModelBase<LocalWrapper>
    {
        public LocalPaneViewModel(ModuleViewModelBase parent, LocalWrapper localWrapper) : base(parent, localWrapper)
        {
        }

        internal override List<FileSystemItem> ChangeDirectory(string selectedPath = null)
        {
            var content = new List<FileSystemItem>();
            var recognize = false;
            if (selectedPath == null)
            {
                recognize = true;
                selectedPath = CurrentFolder.Path;
            }

            foreach (var di in Directory.GetDirectories(selectedPath))
            {
                var item = new FileSystemItem
                    {
                        TitleId = Path.GetFileName(di),
                        Type = ItemType.Directory,
                        Date = Directory.GetLastWriteTime(di),
                        Path = string.Format("{0}\\", di),
                        Thumbnail =
                            recognize ? ApplicationExtensions.GetContentByteArray("/Resources/folder.png") : null
                    };
                //if (recognize) TitleManager.RecognizeTitle(item, CurrentFolder);
                content.Add(item);
            }

            foreach (var fi in Directory.GetFiles(selectedPath))
            {
                var item = new FileSystemItem
                    {
                        Title = Path.GetFileName(fi),
                        Type = ItemType.File,
                        Date = File.GetLastWriteTime(fi),
                        Path = fi,
                        Size = new FileInfo(fi).Length,
                        Thumbnail = recognize ? ApplicationExtensions.GetContentByteArray("/Resources/file.png") : null
                    };
                //if (recognize) TitleManager.RecognizeTitle(item, CurrentFolder);
                content.Add(item);
            }

            return content;
        }

        protected override long CalculateSize(string path)
        {
            var files = Directory.GetFiles(path);
            var directories = Directory.GetDirectories(path);
            return files.Sum(f => new FileInfo(f).Length) + directories.Sum(d => CalculateSize(d));
        }

        public override void LoadDataAsync(LoadCommand cmd, object cmdParam)
        {
            switch (cmd)
            {
                case LoadCommand.Load:
                    Drives = DriveInfo.GetDrives().Select(drive => new FileSystemItemViewModel(new FileSystemItem
                                                                                                   {
                                                                                                       Path = drive.Name,
                                                                                                       Title = drive.Name.TrimEnd('\\'),
                                                                                                       Type = ItemType.Drive
                                                                                                   })).ToObservableCollection();
                    Drive = Drives.First();
                    break;
            }
        }

        public override bool Delete(FileSystemItemViewModel item)
        {
            //TODO: handle errors

            if (item.Type == ItemType.Directory)
            {
                Directory.Delete(item.Path, true);
            }
            else
            {
                File.Delete(item.Path);
            }
            return true;            
        }

        public override bool CreateFolder(string name)
        {
            var path = Path.Combine(CurrentFolder.Path, name);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return true;
        }

        private static DriveInfo GetDriveInfo(FileSystemItemViewModel drive)
        {
            return DriveInfo.GetDrives().First(d => d.Name == drive.Path);
        }

        protected override bool IsDriveAccessible(FileSystemItemViewModel drive)
        {
            var driveInfo = GetDriveInfo(drive);
            if (!driveInfo.IsReady)
            {
                MessageBox.Show(string.Format("{0} is not accessible.", drive.Path));
                return false;
            }
            return true;
        }

        protected override void ChangeDrive()
        {
            var driveInfo = GetDriveInfo(Drive);
            DriveLabel = string.Format("[{0}]", driveInfo.VolumeLabel);
            FreeSpace = string.Format("{0:#,0} of {1:#,0} bytes free", driveInfo.AvailableFreeSpace, driveInfo.TotalSize);
            base.ChangeDrive();
        }
    }
}