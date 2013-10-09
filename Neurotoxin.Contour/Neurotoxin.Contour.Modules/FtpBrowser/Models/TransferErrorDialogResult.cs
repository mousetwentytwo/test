using Neurotoxin.Contour.Modules.FtpBrowser.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    public class TransferErrorDialogResult
    {
        public CopyBehavior Behavior { get; private set; }
        public CopyBehaviorScope Scope { get; private set; }

        public TransferErrorDialogResult(CopyBehavior behavior, CopyBehaviorScope scope)
        {
            Behavior = behavior;
            Scope = scope;
        }
    }
}