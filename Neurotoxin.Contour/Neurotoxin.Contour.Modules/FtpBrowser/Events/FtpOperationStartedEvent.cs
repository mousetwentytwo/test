﻿namespace Neurotoxin.Contour.Modules.FtpBrowser.Events
{
    public delegate void FtpOperationStartedEvent(object sender, FtpOperationStartedEventArgs args);

    public class FtpOperationStartedEventArgs
    {
        public bool BinaryTransfer { get; private set; }

        public FtpOperationStartedEventArgs(bool binaryTransfer)
        {
            BinaryTransfer = binaryTransfer;
        }
    }
}