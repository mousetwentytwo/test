using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Unity;
using Neurotoxin.Godspeed.Core.Extensions;
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

        protected override DependencyObject CreateShell()
        {
            var view = InitializeShell();
            var viewModel = (FileManagerViewModel) view.DataContext;
            viewModel.InitializePanes();
            Application.Current.MainWindow = view;
            UIThread.Run(view.Show);
            CheckPrerequisites(viewModel);
            if (UserSettings.UseVersionChecker) CheckNewerVersion();
            return view;
        }

        private Window InitializeShell()
        {
            return Container.Resolve<FileManagerWindow>();
        }

        private void CheckPrerequisites(FileManagerViewModel viewModel)
        {
            var applicationAssembly = Assembly.GetAssembly(typeof(Application));
            var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
            var actualVersion = new Version(fvi.ProductVersion);
            var requiredVersion = new Version(4, 0, 30319, 18408);

            viewModel.DataGridSupportsRenaming = actualVersion >= requiredVersion;
            if (!viewModel.DataGridSupportsRenaming)
            {
                NotificationMessage.ShowMessage("Warning!", "Some of the features require .NET version 4.0.30319.18408 (October 2013) or newer. Please update .NET Framework and restart GODspeed to enable those features.", NotificationMessageFlags.Ignorable);
            }
        }

        private void CheckNewerVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var title = asm.GetAttribute<AssemblyTitleAttribute>().Title;
            const string url = "https://godspeed.codeplex.com/";
            WorkerThread.Run(() =>
            {
                try
                {
                    var request = HttpWebRequest.Create(url);
                    var response = request.GetResponse();
                    var titlePattern = new Regex(@"\<span class=""rating_header""\>current.*?\<td\>(.*?)\</td\>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var datePattern = new Regex(@"\<span class=""rating_header""\>date.*?\<td\>.*?LocalTimeTicks=""(.*?)""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    string html;
                    using (var stream = response.GetResponseStream())
                    {
                        var sr = new StreamReader(stream, UTF8Encoding.UTF8);
                        html = sr.ReadToEnd();
                        sr.Close();
                    }
                    var latestTitle = titlePattern.Match(html).Groups[1].Value.Trim();
                    var latestDate = new DateTime(1970, 1, 1);
                    latestDate = latestDate.AddSeconds(long.Parse(datePattern.Match(html).Groups[1].Value)).ToLocalTime();
                    return new Tuple<string, DateTime>(latestTitle, latestDate);
                }
                catch
                {
                    return new Tuple<string, DateTime>(string.Empty, DateTime.MinValue);
                }
            },
                             info =>
                             {
                                 if ((string.Compare(title, info.Item1, StringComparison.InvariantCultureIgnoreCase) != -1)) return;
                                 var dialog = new NewVersionDialog(string.Format("{0} ({1:yyyy.MM.dd HH:mm})", info.Item1, info.Item2));
                                 dialog.ShowDialog();
                             });
        }

    }
}