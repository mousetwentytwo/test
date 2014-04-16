using System;
using System.Globalization;
using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Controllers;
using Neurotoxin.Godspeed.Shell.Helpers;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using WPFLocalizeExtension.Engine;

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
            Container.RegisterType<StatisticsViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ConnectionsViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<FtpContentViewModel>();
            Container.RegisterType<LocalFileSystemContentViewModel>();
            Container.RegisterType<StfsPackageContentViewModel>();
            Container.RegisterType<CompressedFileContentViewModel>();

            // Views
            Container.RegisterType<FileManagerWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SettingsWindow>();
            Container.RegisterType<StatisticsWindow>();

            // Helpers
            Container.RegisterType<SanityChecker>(new ContainerControlledLifetimeManager());
        }

        protected override IModuleCatalog GetModuleCatalog()
        {
            // Gets the catalog from the app.config
            var catalog = new ConfigurationModuleCatalog();
            catalog.Load();
            return catalog;
        }

        protected override DependencyObject CreateShell()
        {
            LocalizeDictionary.Instance.Culture = UserSettings.Language ?? CultureInfo.CurrentCulture;
            Container.Resolve<SanityChecker>();
            var shell = Container.Resolve<FileManagerWindow>();
            var viewModel = (FileManagerViewModel)shell.DataContext;
            viewModel.Initialize();
            Application.Current.MainWindow = shell;
            UIThread.Run(shell.Show);
            return shell;
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
    }
}