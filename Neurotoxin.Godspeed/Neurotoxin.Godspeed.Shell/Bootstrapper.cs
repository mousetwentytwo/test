using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Controllers;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            UnityInstance.Container = Container;
            Container.RegisterType<IGeneralController, ModuleController>(new ContainerControlledLifetimeManager());

            Container.RegisterType<FtpContent>();
            Container.RegisterType<LocalFileSystemContent>();
            Container.RegisterType<StfsPackageContent>();

            Container.RegisterType<FileManagerView>(new ContainerControlledLifetimeManager());
            Container.RegisterType<FileManagerViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ConnectionsViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<FtpContentViewModel>();
            Container.RegisterType<LocalFileSystemContentViewModel>();
            Container.RegisterType<StfsPackageContentViewModel>();
            Container.RegisterType<CacheManager>(new ContainerControlledLifetimeManager());
        }

        protected override IModuleCatalog GetModuleCatalog()
        {
            // Gets the catalog from the app.config
            var catalog = new ConfigurationModuleCatalog();
            catalog.Load();
            return catalog;
        }

        private IEnumerable<IGeneralController> GetControllers()
        {
            return new IGeneralController[]
                       {
                           Container.Resolve<ModuleController>()
                       };
        }

        protected override DependencyObject CreateShell()
        {
            var view = InitializeShell();
            ((FileManagerViewModel) view.DataContext).InitializePanes();
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

            return Container.Resolve<FileManagerView>();
        }
    }
}