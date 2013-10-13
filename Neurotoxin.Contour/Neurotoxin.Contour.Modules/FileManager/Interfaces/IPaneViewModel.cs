using System;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface IPaneViewModel
    {
        bool IsActive { get; }
        void SetActive();
        void Refresh();
        void LoadDataAsync(LoadCommand cmd, object cmdParam, Action success = null, Action error = null);
    }
}