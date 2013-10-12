using Neurotoxin.Contour.Modules.FileManager.Models;

namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface ITransferErrorDialog
    {
        bool? ShowDialog();
        TransferErrorDialogResult Result { get; }
    }
}