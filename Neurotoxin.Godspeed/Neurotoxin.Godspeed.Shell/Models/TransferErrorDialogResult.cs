using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
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