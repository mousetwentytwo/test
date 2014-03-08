using System;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Exceptions
{
    public class TransferException : Exception
    {
        public TransferErrorType Type { get; private set; }
        public string SourceFile { get; private set; }
        public string TargetFile { get; private set; }
        public long TargetFileSize { get; private set; }

        public TransferException(TransferErrorType type, string message, Exception innerException) 
            : this(type, message, null, null, 0, innerException)
        {
        }

        public TransferException(TransferErrorType type, string message, string sourceFile = null, string targetFile = null, long targetFileSize = 0, Exception innerException = null)
            : base(message, innerException)
        {
            Type = type;
            SourceFile = sourceFile;
            TargetFile = targetFile;
            TargetFileSize = targetFileSize;
        }
    }
}
