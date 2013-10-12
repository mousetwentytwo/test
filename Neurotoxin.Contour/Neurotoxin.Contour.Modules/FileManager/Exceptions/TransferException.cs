using System;
using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Exceptions
{
    public class TransferException : Exception
    {
        public TransferErrorType Type { get; private set; }
        public string SourceFile { get; private set; }
        public string TargetFile { get; private set; }

        public TransferException(TransferErrorType type, string sourceFile, string targetFile, Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Type = type;
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }

        public TransferException(TransferErrorType type, string message, params object[] args)
            : this(type, null, null, string.Format(message, args))
        {
        }

        public TransferException(TransferErrorType type, string sourceFile, string targetFile, string message, params object[] args)
            : base(string.Format(message, args))
        {
            Type = type;
            SourceFile = sourceFile;
            TargetFile = targetFile;
        }
    }
}
