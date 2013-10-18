using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public interface IModulePresentation
    {
        /// <summary>
        /// Current view presented by the module.
        /// </summary>
        IView GetView(string viewName);

        /// <summary>
        /// Get the StatusBar to the current view of the module
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        UserControl GetStatusBar(string viewName);
    }
}