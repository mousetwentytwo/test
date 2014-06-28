using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class MigrationFinishedEvent : CompositePresentationEvent<MigrationFinishedEventArgs> { }

    public class MigrationFinishedEventArgs
    {
        public IProgressViewModel ViewModel { get; private set; }

        public MigrationFinishedEventArgs(IProgressViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}