using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FtpOperationProgressChangedEvent : CompositePresentationEvent<FtpOperationProgressChangedEventArgs> {}

    public class FtpOperationProgressChangedEventArgs
    {
        public int Percentage { get; private set; }

        public FtpOperationProgressChangedEventArgs(int percentage)
        {
            Percentage = percentage;
        }
    }
}