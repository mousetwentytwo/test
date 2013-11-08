using System;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IPaneViewModel : IDisposable
    {
        bool IsBusy { get; }
        string ProgressMessage { get; }
        bool IsActive { get; }
        void SetActive();
        void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null);
    }
}