using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class TransferStartedEvent : CompositePresentationEvent<TransferStartedEventArgs> { }

    public class TransferStartedEventArgs
    {
        public TransferManagerViewModel Sender { get; private set; }

        public TransferStartedEventArgs(TransferManagerViewModel sender)
        {
            Sender = sender;
        }
    }
}