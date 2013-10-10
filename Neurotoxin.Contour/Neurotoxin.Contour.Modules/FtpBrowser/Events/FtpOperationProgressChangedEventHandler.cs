namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
{
    public delegate void FtpOperationProgressChangedEventHandler(object sender, FtpOperationProgressChangedEventArgs args);

    public class FtpOperationProgressChangedEventArgs
    {
        public int Percentage { get; private set; }

        public FtpOperationProgressChangedEventArgs(int percentage)
        {
            Percentage = percentage;
        }
    }
}