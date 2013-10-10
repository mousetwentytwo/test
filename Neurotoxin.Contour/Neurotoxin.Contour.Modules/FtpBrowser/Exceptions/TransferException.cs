using System;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Exceptions
{
    public class TransferException : Exception
    {
        public TransferErrorType Type { get; private set; }

        public TransferException(TransferErrorType type, Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Type = type;
        }

        public TransferException(TransferErrorType type, string message, params object[] args)
            : base(string.Format(message, args))
        {
            Type = type;
        }
    }
}
