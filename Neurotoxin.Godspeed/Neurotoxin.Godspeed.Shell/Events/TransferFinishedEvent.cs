using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferFinishedEvent : CompositePresentationEvent<TransferFinishedEventArgs> { }

    public class TransferFinishedEventArgs
    {
        public TransferManagerViewModel Sender { get; private set; }

        public TransferFinishedEventArgs(TransferManagerViewModel sender)
        {
            Sender = sender;
        }
    }
}