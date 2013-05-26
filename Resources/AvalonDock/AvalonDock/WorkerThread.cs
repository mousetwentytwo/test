using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace AvalonDock
{
    public static class WorkerThread
    {
        delegate void Job();

        private static Action clientCallback;

        public static void Run(Action work, Action callback)
        {
            Job j = new Job(work);
            clientCallback = callback;
            j.BeginInvoke(Callback, null);
        }

        private static void Callback(IAsyncResult result)
        {
            Job j = (Job)((AsyncResult)result).AsyncDelegate;
            j.EndInvoke(result);
            UIThread.Run(clientCallback);
        }
    }

    public static class UIThread
    {
        public static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

        public static bool IsUIThread
        {
            get
            {
                return Dispatcher.CheckAccess();  
            }
        }

        public static void Run(Action work)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(work, new object[0]);
            } else
            {
                work();
            }
        }

        public static void RunSync(Action work)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(work, new object[0]);
            }
            else
            {
                work();
            }
        }
    }
}