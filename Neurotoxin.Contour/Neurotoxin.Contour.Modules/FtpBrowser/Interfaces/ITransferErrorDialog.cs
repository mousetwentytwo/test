using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Interfaces
{
    public interface ITransferErrorDialog
    {
        bool? ShowDialog();
        TransferErrorDialogResult Result { get; }
    }
}