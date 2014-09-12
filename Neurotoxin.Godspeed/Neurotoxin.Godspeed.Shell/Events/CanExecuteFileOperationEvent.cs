using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class CanExecuteFileOperationEvent : CompositePresentationEvent<CanExecuteFileOperationEventArgs> { }

    public class CanExecuteFileOperationEventArgs : CancelableEventArgs
    {
        public CanExecuteFileOperationEventArgs(object sender) : base(sender)
        {
        }
    }
}