namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
{
    public delegate void FtpOperationProgressChangedEvent(object sender, FtpOperationProgressChangedEventArgs args);

    public class FtpOperationProgressChangedEventArgs
    {
        public int Percentage { get; private set; }

        public FtpOperationProgressChangedEventArgs(int percentage)
        {
            Percentage = percentage;
        }
    }
}