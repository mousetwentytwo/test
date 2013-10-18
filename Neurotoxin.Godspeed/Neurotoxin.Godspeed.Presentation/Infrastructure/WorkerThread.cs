﻿using System;
using System.Windows;
using System.Windows.Threading;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public static class WorkerThread
    {
        public static void Run<T>(Func<T> work, Action<T> success, Action<Exception> error = null)
        {
            work.BeginInvoke(asyncResult =>
                {
                    try
                    {
                        var result = work.EndInvoke(asyncResult);
                        UIThread.Run(success, result);
                    }
                    catch (Exception ex)
                    {
                        if (error != null) UIThread.Run(error, ex);
                    }
                
            }, null);
        }
    }

    public static class UIThread
    {
        private static readonly Dispatcher Dispatcher = Application.Current.Dispatcher;

        public static bool IsUIThread
        {
            get
            {
                return Dispatcher.CheckAccess();  
            }
        }

        public static DispatcherOperation BeginRun(Action work)
        {
            return Dispatcher.BeginInvoke(work);
        }

        public static DispatcherOperation BeginRun(Action work, DispatcherPriority priority)
        {
            return Dispatcher.BeginInvoke(work,priority);
        }

        public static void Run(Action work)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(work);
            }
            else
            {
                work();
            }
        }

        public static void Run<T>(Action<T> work, T p1)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(work, p1);
            } 
            else
            {
                work(p1);
            }
        }

        public static void RunSync(Action work)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(work);
            }
            else
            {
                work();
            }
        }
    }
}