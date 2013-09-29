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
                var parentFolder = Stack.ElementAt(1);
                content.Add(new FileSystemItem
                {
                    Title = "[..]",
                    Type = parentFolder.Type,
                    Date = parentFolder.Date,
                    Path = parentFolder.Path,
                    Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/up.png")
                });
            }
            var selectedPath = CurrentFolder.Path;
            var directories = Directory.GetDirectories(selectedPath);
            content.AddRange(directories.Select(di => new FileSystemItem
                                                          {
                                                              TitleId = Path.GetFileName(di),
                                                              Type = ItemType.Directory,
                                                              Date = Directory.GetLastWriteTime(di),
                                                              Path = di,
                                                              Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/folder.png")
                                                          }));
            var files = Directory.GetFiles(selectedPath);
            content.AddRange(files.Select(fi => new FileSystemItem
                                                    {
                                                        Title = Path.GetFileName(fi),
                                                        Type = ItemType.File,
                                                        Date = File.GetLastWriteTime(fi),
                                                        Path = fi,
                                                        Size = new FileInfo(fi).Length,
                                                        Thumbnail = ApplicationExtensions.GetContentByteArray("/Resources/file.png")
                                                    }));
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
                    Stack = new Stack<FileSystemItem>();
                    var root = new FileSystemItem
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