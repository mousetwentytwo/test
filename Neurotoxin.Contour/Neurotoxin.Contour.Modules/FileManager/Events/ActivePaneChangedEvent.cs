using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Modules.FileManager.ViewModels;

namespace Neurotoxin.Contour.Modules.FileManager.Events
{
    public class ActivePaneChangedEvent : CompositePresentationEvent<ActivePaneChangedEventArgs> { }

    public class ActivePaneChangedEventArgs
    {
        public IPaneViewModel ActivePane { get; private set; }

        public ActivePaneChangedEventArgs(IPaneViewModel activePane)
        {
            ActivePane = activePane;
        }
    }
}