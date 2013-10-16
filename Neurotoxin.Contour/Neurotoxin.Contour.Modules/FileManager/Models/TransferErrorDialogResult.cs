using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    public class TransferErrorDialogResult
    {
        public ErrorResolutionBehavior Behavior { get; private set; }
        public CopyAction? Action { get; private set; }
        public CopyActionScope Scope { get; private set; }

        public TransferErrorDialogResult(ErrorResolutionBehavior behavior, CopyActionScope scope = Constants.CopyActionScope.Current, CopyAction? action = null)
        {
            Behavior = behavior;
            Scope = scope;
            Action = action;
        }
    }
}