using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;
using Mti.Mnp.Client.Wpf.Shell;
using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Shell.Controllers;
using Neurotoxin.Contour.Shell.Views;

namespace Neurotoxin.Contour.Shell
{
    /// <summary>
    /// The bootstrapper of this Prism-based app.
    /// </summary>
    public class Bootstrapper : UnityBootstrapper
    {
        private readonly EnterpriseLibraryLogger logger = new EnterpriseLibraryLogger();

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            UnityInstance.Container = Container;
            // Singleton ShellView
            // ModuleController is responsible for switching modules displayed in specified regions with the given view
            Container.RegisterType<IGeneralController, ModuleController>(new ContainerControlledLifetimeManager());
            // TabController is responsible for render modules in tabs
            Container.RegisterType<ITabController, TabController>(new ContainerControlledLifetimeManager());
        }

        protected override IModuleCatalog GetModuleCatalog()
        {
            // Gets the catalog from the app.config
            var catalog = new ConfigurationModuleCatalog();
            catalog.Load();
            return catalog;
        }

        protected override ILoggerFacade LoggerFacade
        {
            get { return logger; }
        }

        //protected override void InitializeModules()
        //{
        //    base.InitializeModules();
        //}

        private IEnumerable<IGeneralController> GetControllers()
        {
            return new IGeneralController[]
                       {
                           Container.Resolve<TabController>(),
                           Container.Resolve<ModuleController>()
                       };
        }

        protected override DependencyObject CreateShell()
        {
            var view = InitializeShell();
            Application.Current.MainWindow = view;
            UIThread.Run(view.Show);
            return view;
        }

        private Window InitializeShell()
        {
            var controllers = GetControllers();
            if (controllers != null)
            {
                foreach (var controller in controllers)
                    controller.Run();
            }

            return Container.Resolve<ShellView>();
        }
    }
}