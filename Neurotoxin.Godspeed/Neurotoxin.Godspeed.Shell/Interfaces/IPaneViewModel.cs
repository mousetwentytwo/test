using System;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IPaneViewModel
    {
        bool IsActive { get; }
        void SetActive();
        void Refresh();
        void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase> error = null);
    }
}