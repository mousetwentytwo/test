using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class RaiseCanExecuteChangesEvent : CompositePresentationEvent<RaiseCanExecuteChangesEventArgs> { }

    public class RaiseCanExecuteChangesEventArgs
    {
        public CommonViewModelBase Sender { get; private set; }

        public RaiseCanExecuteChangesEventArgs(CommonViewModelBase sender)
        {
            Sender = sender;
        }
    }
}