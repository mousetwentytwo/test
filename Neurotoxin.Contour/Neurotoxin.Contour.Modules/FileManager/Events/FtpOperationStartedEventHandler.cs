﻿namespace Neurotoxin.Contour.Modules.FileManager.Events
{
    public delegate void FtpOperationStartedEventHandler(object sender, FtpOperationStartedEventArgs args);

    public class FtpOperationStartedEventArgs
    {
        public bool BinaryTransfer { get; private set; }

        public FtpOperationStartedEventArgs(bool binaryTransfer)
        {
            BinaryTransfer = binaryTransfer;
        }
    }
}