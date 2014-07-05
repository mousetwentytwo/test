using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FreestyleDatabaseCheckedEvent : CompositePresentationEvent<FreestyleDatabaseCheckedEventArgs> { }

    public class FreestyleDatabaseCheckedEventArgs
    {
        public FreestyleDatabaseCheckerViewModel ViewModel { get; private set; }

        public FreestyleDatabaseCheckedEventArgs(FreestyleDatabaseCheckerViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}