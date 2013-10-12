using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using Neurotoxin.Contour.Modules.FileManager.Views;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager
{
    public static class ModuleDescription
    {
        public const string Name = "FileManager";
    }

    [Module(ModuleName = ModuleDescription.Name)]
    public class ModuleInit : ModuleInitBase
    {
        public ModuleInit(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator) : base(container, regionManager, eventAggregator) { }

        #region IModule Members

        public override void Initialize()
        {
            // This class exposes IModulePresentation interface that can be resolved by the module name
            container.RegisterInstance<IModulePresentation>(ModuleDescription.Name, this);

            // IView interface can be resolved by the view name and implemented by the ProfileEditorView class
            container.RegisterType<FileManagerView>();
        }

        #endregion IModule Members

        #region IModulePresentation Members

        public override IView GetView(string viewName)
        {
            return container.Resolve<FileManagerView>();
        }

        #endregion IModulePresentation Members
    }
}