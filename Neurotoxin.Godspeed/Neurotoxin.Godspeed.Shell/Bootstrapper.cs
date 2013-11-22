using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Controllers;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;

namespace Neurotoxin.Godspeed.Shell
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            UnityInstance.Container = Container;
            Container.RegisterType<IGeneralController, ModuleController>(new ContainerControlledLifetimeManager());

            // Content providers
            Container.RegisterType<FtpContent>();
            Container.RegisterType<LocalFileSystemContent>();
            Container.RegisterType<StfsPackageContent>();
            Container.RegisterType<CompressedFileContent>();
            Container.RegisterType<CacheManager>(new ContainerControlledLifetimeManager());

            // ViewModels
            Container.RegisterType<FileManagerViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SettingsViewModel>();
            Container.RegisterType<ConnectionsViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<FtpContentViewModel>();
            Container.RegisterType<LocalFileSystemContentViewModel>();
            Container.RegisterType<StfsPackageContentViewModel>();
            Container.RegisterType<CompressedFileContentViewModel>();

            // Views
            Container.RegisterType<FileManagerWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SettingsWindow>();
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

            return Container.Resolve<FileManagerWindow>();
        }
    }
}