using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferFinishedEvent : CompositePresentationEvent<TransferFinishedEventArgs> { }

    public class TransferFinishedEventArgs : EventArgsBase
    {
        public TransferFinishedEventArgs(object sender) : base(sender)
        {
        }
    }
}