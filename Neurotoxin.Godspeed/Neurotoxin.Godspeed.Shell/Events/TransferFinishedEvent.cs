using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferFinishedEvent : CompositePresentationEvent<TransferFinishedEventArgs> { }

    public class TransferFinishedEventArgs : EventArgsBase
    {
        public bool Shutdown { get; set; }
        public IFileListPaneViewModel Source { get; private set; }
        public IFileListPaneViewModel Target { get; private set; }

        public TransferFinishedEventArgs(object sender, IFileListPaneViewModel source, IFileListPaneViewModel target) : base(sender)
        {
            Source = source;
            Target = target;
        }

    }
}