using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Models;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class ExecuteFileOperationEvent : CompositePresentationEvent<ExecuteFileOperationEventArgs> { }

    public class ExecuteFileOperationEventArgs
    {
        public FileOperation Action { get; private set; }
        public IFileListPaneViewModel SourcePane { get; private set; }
        public IList<FileSystemItem> Items { get; private set; }

        public ExecuteFileOperationEventArgs(FileOperation action, IFileListPaneViewModel sourcePane, IList<FileSystemItem> items)
        {
            Action = action;
            SourcePane = sourcePane;
            Items = items;
        }
    }
}