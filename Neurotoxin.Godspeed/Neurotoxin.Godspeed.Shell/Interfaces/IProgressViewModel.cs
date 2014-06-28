using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IProgressViewModel : IViewModel
    {
        string ProgressDialogTitle { get; }
        string ProgressMessage { get; }
        int ProgressValue { get; }
        double ProgressValueDouble { get; }
        bool IsIndetermine { get; }

        //event WorkFinishedEventHandler Finished;
    }
}