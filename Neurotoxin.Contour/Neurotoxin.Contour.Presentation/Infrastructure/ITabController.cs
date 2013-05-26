using Neurotoxin.Contour.Presentation.Controls;

namespace Neurotoxin.Contour.Presentation.Infrastructure
{
    public interface ITabController : IGeneralController
    {
        void DetachDockableTabItem(DockableTabItem tabItem);
        void RegisterItem(DockableTabItem tabItem);
    }
}