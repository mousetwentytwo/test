using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
{
    public class TransferStartedEvent : CompositePresentationEvent<TransferStartedEventArgs> { }

    public class TransferStartedEventArgs
    {
    }
}