using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Shell.Reporting;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Neurotoxin.Godspeed.Shell.Views;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell
{

    public partial class App : Application
    {
        private Bootstrapper _bootstrapper;

        public static string DataDirectory { get; private set; }

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
            DataDirectory = string.Format(@"{0}\{1}\{2}\{3}", appData, company, product, version);
            if (!Directory.Exists(DataDirectory)) Directory.CreateDirectory(DataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);
            var tempDir = Path.Combine(DataDirectory, "temp");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
            var postDir = Path.Combine(DataDirectory, "post");
            if (!Directory.Exists(postDir)) Directory.CreateDirectory(postDir);
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
            Shutdown(ex.GetType().Name.GetHashCode());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var fileManager = _bootstrapper.Resolve<FileManagerViewModel>();
            if (!fileManager.IsDisposed) fileManager.Dispose();
            var statistics = _bootstrapper.Resolve<StatisticsViewModel>();
            if (e.ApplicationExitCode != 0) statistics.ApplicationCrashed++;

            statistics.PersistData();

            if (UserSettings.DisableUserStatisticsParticipation != false)
            {
                var commandUsage = new StringBuilder();
                foreach (var kvp in statistics.CommandUsage)
                {
                    commandUsage.AppendLine(string.Format("{0}={1}", kvp.Key, kvp.Value));
                }

                var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(statistics.UsageStart);
                HttpForm.Post("stats.php", new List<IFormData>
                {
                    new RawPostData("client_id", EsentPersistentDictionary.Instance.Get<string>("ClientID")),
                    new RawPostData("version", GetApplicationVersion()),
                    new RawPostData("wpf", GetFrameworkVersion()),
                    new RawPostData("os", Environment.OSVersion.VersionString),
                    new RawPostData("culture", CultureInfo.CurrentCulture.Name),
                    new RawPostData("uiculture", CultureInfo.CurrentUICulture.Name),
                    new RawPostData("osculture", CultureInfo.InstalledUICulture.Name),
                    new RawPostData("date", statistics.UsageStart.ToUnixTimestamp()),
                    new RawPostData("timezone", string.Format("{0}{1:D2}:{2:D2}", utcOffset.Hours >= 0 ? "+" : string.Empty, utcOffset.Hours, utcOffset.Minutes)),
                    new RawPostData("usage", Math.Floor(statistics.UsageTime.TotalSeconds)),
                    new RawPostData("exit_code", e.ApplicationExitCode),
                    new RawPostData("games_recognized", statistics.GamesRecognizedFully),
                    new RawPostData("partially_recognized", statistics.GamesRecognizedPartially),
                    new RawPostData("svod_recognized", statistics.SvodPackagesRecognized),
                    new RawPostData("stfs_recognized", statistics.StfsPackagesRecognized),
                    new RawPostData("transferred_bytes", statistics.BytesTransferred),
                    new RawPostData("transferred_files", statistics.FilesTransferred),
                    new RawPostData("transfer_time", Math.Floor(statistics.TimeSpentWithTransfer.TotalSeconds)),
                    new RawPostData("command_usage", commandUsage)
                });
            }
            base.OnExit(e);
        }

        public static string GetApplicationVersion()
        {
            var assembly = Assembly.GetAssembly(typeof(FileManagerWindow));
            var assemblyName = assembly.GetName();
            return assemblyName.Version.ToString();
        }

        public static string GetFrameworkVersion()
        {
            var applicationAssembly = Assembly.GetAssembly(typeof(Application));
            var fvi = FileVersionInfo.GetVersionInfo(applicationAssembly.Location);
            return fvi.ProductVersion;
        }

    }
}