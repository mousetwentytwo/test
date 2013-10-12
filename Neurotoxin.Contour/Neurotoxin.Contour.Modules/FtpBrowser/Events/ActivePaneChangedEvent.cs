using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.Interfaces;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
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