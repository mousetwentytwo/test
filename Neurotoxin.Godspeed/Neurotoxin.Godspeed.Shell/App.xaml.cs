using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.UnityExtensions;
using Neurotoxin.Godspeed.Presentation.Events;
using Neurotoxin.Godspeed.Core.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;

namespace Neurotoxin.Godspeed.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UnityBootstrapper bootstrapper;
        private IEventAggregator eventAggregator;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var asm = Assembly.GetExecutingAssembly();
            CheckNewerVersion(asm);
            SetDataDirectory(asm);
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
            bootstrapper = new Bootstrapper();
            bootstrapper.Run();
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

        private void CheckNewerVersion(Assembly asm)
        {
            var title = asm.GetAttribute<AssemblyTitleAttribute>().Title;
            var installDate = File.GetLastWriteTime(asm.Location);
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
                                     if ((string.Compare(title, info.Item1, StringComparison.InvariantCultureIgnoreCase) != -1 && info.Item2 <= installDate)) return;
                                     var dialog = new NewVersionDialog(string.Format("{0} ({1:yyyy.MM.dd HH:mm})", info.Item1, info.Item2));
                                     dialog.ShowDialog();
                                 });
        }

        private void SetDataDirectory(Assembly asm)
        {
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

        public void HandleException(Exception ex)
        {
            //if (eventAggregator == null) eventAggregator = bootstrapper.Container.Resolve<IEventAggregator>();

            var raisedex = ex is TargetInvocationException ? ex.InnerException : ex;
            NotificationMessage.Show("Error", raisedex.Message);
            Shutdown();

            //eventAggregator.GetEvent<ExceptionEvent>().Publish(raisedex);
        }

    }
}