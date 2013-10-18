using System.Windows.Controls;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Events;
using Neurotoxin.Godspeed.Presentation.Controls;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public abstract class ModuleInitBase : IModule, IModulePresentation
    {
        protected readonly IUnityContainer container;
        protected readonly IRegionManager regionManager;
        protected readonly IEventAggregator eventAggregator;

        public ModuleInitBase(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
        }

        #region IModule Members

        public virtual void Initialize()
        {
        }

        #endregion

        #region IModulePresentation Members

        public abstract IView GetView(string viewName);

        public virtual UserControl GetStatusBar(string viewName)
        {
            return new StatusBar();
        }

        #endregion
    }
}