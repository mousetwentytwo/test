using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.Events
{
    public class ViewModelGeneratedEvent : CompositePresentationEvent<ViewModelGeneratedEventArgs> { }

    public class ViewModelGeneratedEventArgs
    {
        public ViewModelBase ViewModel { get; private set; }

        public ViewModelGeneratedEventArgs(ViewModelBase viewModel)
        {
            ViewModel = viewModel;
        }
    }
}