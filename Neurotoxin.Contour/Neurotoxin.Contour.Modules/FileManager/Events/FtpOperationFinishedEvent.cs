using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Contour.Modules.FileManager.Events
{
    public class FtpOperationFinishedEvent : CompositePresentationEvent<FtpOperationFinishedEventArgs> { }

    public class FtpOperationFinishedEventArgs
    {
        public long? StreamLength { get; private set; }

        public FtpOperationFinishedEventArgs(long? streamLength)
        {
            StreamLength = streamLength;
        }
    }
}