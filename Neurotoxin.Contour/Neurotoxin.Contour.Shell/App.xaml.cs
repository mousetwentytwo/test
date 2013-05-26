using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.UnityExtensions;
using Neurotoxin.Contour.Presentation.Events;

namespace Neurotoxin.Contour.Shell
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
            Dispatcher.CurrentDispatcher.UnhandledException += UnhandledThreadingException;
#if (DEBUG)
            RunInDebugMode();
#else
              RunInReleaseMode();
#endif
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
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
            if (eventAggregator == null) eventAggregator = bootstrapper.Container.Resolve<IEventAggregator>();

            Exception raisedex = ex is TargetInvocationException ? ex.InnerException : ex;
            eventAggregator.GetEvent<ExceptionEvent>().Publish(raisedex);
        }

    }
}