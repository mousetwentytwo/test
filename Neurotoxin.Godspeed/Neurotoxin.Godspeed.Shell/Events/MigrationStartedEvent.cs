using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class MigrationStartedEvent : CompositePresentationEvent<MigrationStartedEventArgs> { }

    public class MigrationStartedEventArgs
    {
        public IProgressViewModel ViewModel { get; private set; }

        public MigrationStartedEventArgs(IProgressViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}