using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class LocalPaneViewModel : PaneViewModelBase
    {
        public LocalPaneViewModel(ModuleViewModelBase parent) : base(parent)
        {
        }

        protected override List<FileSystemItem> ChangeDirectory()
        {
            var content = new List<FileSystemItem>();
            if (Stack.Count > 1)
            {
                content.Add(new FileSystemItem
                    {
                        Title = "[..]",
                        Type = Selection.Type,
                        Date = Selection.Date,
                        Path = Path.GetDirectoryName(SelectedPath),
                        Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/up.png")
                    });
            }
            foreach (var di in Directory.GetDirectories(SelectedPath))
            {
                var name = Path.GetFileName(di);
                var directory = new FileSystemItem
                {
                    TitleId = name,
                    Type = ItemType.Directory,
                    Date = Directory.GetLastWriteTime(di),
                    Path = di,
                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png")
                };

                content.Add(directory);
            }
            foreach (var fi in Directory.GetFiles(SelectedPath))
            {
                var name = Path.GetFileName(fi);
                var file = new FileSystemItem
                    {
                        Title = name,
                        Type = ItemType.File,
                        Date = File.GetLastWriteTime(fi),
                        Path = fi,
                        Size = new FileInfo(fi).Length,
                        Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/file.png")
                    };
                content.Add(file);
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
                    Drive = "C:";
                    Stack = new Stack<FileSystemItemViewModel>();
                    var root = new FileSystemItemViewModel
                        {
                            Path = string.Format("{0}\\", Drive),
                            Title = Drive,
                            Type = ItemType.Directory
                        };
                    Stack.Push(root);
                    ChangeDirectoryCommand.Execute(root.Path);
                    break;
            }
        }

        public override void DeleteAll()
        {
            throw new NotImplementedException();
        }

        public override void CreateFolder(string name)
        {
            throw new NotImplementedException();
        }
    }
}