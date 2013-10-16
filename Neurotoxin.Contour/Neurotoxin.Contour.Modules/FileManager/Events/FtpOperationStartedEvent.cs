using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Contour.Modules.FileManager.Events
{
    public class FtpOperationStartedEvent : CompositePresentationEvent<FtpOperationStartedEventArgs> { }

    public class FtpOperationStartedEventArgs
    {
        public bool BinaryTransfer { get; private set; }

        public FtpOperationStartedEventArgs(bool binaryTransfer)
        {
            BinaryTransfer = binaryTransfer;
        }
    }
}