namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
{
    public delegate void FtpOperationFinishedEventHandler(object sender, FtpOperationFinishedEventArgs args);

    public class FtpOperationFinishedEventArgs
    {
        public long? StreamLength { get; private set; }

        public FtpOperationFinishedEventArgs(long? streamLength)
        {
            StreamLength = streamLength;
        }
    }
}