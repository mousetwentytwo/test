using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Limilabs.FTP.Client;
using Microsoft.Practices.Composite.UnityExtensions;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UnityBootstrapper _bootstrapper;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

//            var ftpAsm = Assembly.GetAssembly(typeof (Ftp));
//            var c = ftpAsm.GetTypes().FirstOrDefault(t => t.Name == "Class84");
//            var cc = c.GetConstructor(new Type[] {});
//            var c84 = cc.Invoke(null);
//            var mi = c.GetMethods().FirstOrDefault(m => m.Name == "method_0");
//            Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
//            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pl-PL");

//            DateTime dateTime_0;

//            string[] strArrays = new string[] { "MMM d yyyy" };
//            var flag = DateTime.TryParseExact("Cze 22 2039", strArrays, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime_0);

//            var res = mi.Invoke(c84, new object[] { @"drwxrwxrwx   1 root  root    0 Cze 22 2039 Game
//smb
//drwxrwxrwx   1 root  root    0 Sty 01 1970 Flash
//drwxrwxrwx   1 root  root    0 Sty 01 1970 Hdd1
//drwxrwxrwx   1 root  root    0 Sty 01 1970 HddX
//drwxrwxrwx   1 root  root    0 Sty 01 1970 SysExt
//drwxrwxrwx   1 root  root    0 Sty 01 1970 Usb0" });

            SetDataDirectory();
            Dispatcher.CurrentDispatcher.UnhandledException += UnhandledThreadingException;
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
            Shutdown();
        }

    }
}