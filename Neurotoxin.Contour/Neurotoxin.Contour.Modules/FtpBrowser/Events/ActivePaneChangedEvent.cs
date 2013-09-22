using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
{
    public class ActivePaneChangedEvent : CompositePresentationEvent<ActivePaneChangedEventArgs> { }

    public class ActivePaneChangedEventArgs
    {
        public PaneViewModelBase ActivePane { get; private set; }

        public ActivePaneChangedEventArgs(PaneViewModelBase activePane)
        {
            ActivePane = activePane;
        }
    }
}