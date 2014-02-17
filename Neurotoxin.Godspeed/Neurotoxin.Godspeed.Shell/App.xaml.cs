﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell
{

    public partial class App : Application
    {
        private Bootstrapper _bootstrapper;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetDataDirectory();
            Dispatcher.CurrentDispatcher.UnhandledException += UnhandledThreadingException;
            EsentPersistentDictionary.Instance.Set("ClientID", Guid.NewGuid().ToString());
            
            ShutdownMode = ShutdownMode.OnMainWindowClose;

#if (DEBUG)
            RunInDebugMode();
#else
              RunInReleaseMode();
#endif
        }

        private void RunInDebugMode()
        {
            _bootstrapper = new Bootstrapper();
            _bootstrapper.Run();
        }

        private void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            try
            {
                RunInDebugMode();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void SetDataDirectory()
        {
            var asm = Assembly.GetExecutingAssembly();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var company = asm.GetAttribute<AssemblyCompanyAttribute>().Company;
            var product = asm.GetAttribute<AssemblyProductAttribute>().Product;
            var version = asm.GetAttribute<AssemblyFileVersionAttribute>().Version;
            var appDir = string.Format(@"{0}\{1}\{2}\{3}", appData, company, product, version);
            if (!Directory.Exists(appDir)) Directory.CreateDirectory(appDir);
            AppDomain.CurrentDomain.SetData("DataDirectory", appDir);
            var tempDir = Path.Combine(appDir, "temp");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
        }

        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void UnhandledThreadingException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleException(e.Exception);
        }

        private void HandleException(Exception ex)
        {
            ex = ex is TargetInvocationException ? ex.InnerException : ex;
            ErrorMessage.Show(ex);
            //TODO: ex to exitCode
            Shutdown(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var fileManager = _bootstrapper.Resolve<FileManagerViewModel>();
            if (!fileManager.IsDisposed) fileManager.Dispose();
            var statistics = _bootstrapper.Resolve<StatisticsViewModel>();
            if (e.ApplicationExitCode != 0) statistics.ApplicationCrashed++;

            statistics.PersistData();

            HttpForm.Post("http://www.mercenary.hu/godspeed/stats.php", new List<IFormData>
                {
                    new RawPostData("client_id", EsentPersistentDictionary.Instance.Get<string>("ClientID")),
                    new RawPostData("date", statistics.UsageStart.ToUnixTimestamp()),
                    new RawPostData("usage", statistics.UsageTime),
                    new RawPostData("exit_code", e.ApplicationExitCode),
                    new RawPostData("games_recognized", statistics.GamesRecognizedFully),
                    new RawPostData("partially_recognized", statistics.GamesRecognizedPartially),
                    new RawPostData("svod_recognized", statistics.SvodPackagesRecognized),
                    new RawPostData("stfs_recognized", statistics.StfsPackagesRecognized),
                    new RawPostData("transferred_bytes", statistics.BytesTransferred),
                    new RawPostData("transferred_files", statistics.FilesTransferred),
                    new RawPostData("transfer_time", statistics.TimeSpentWithTransfer),
                });
            base.OnExit(e);
        }

    }
}