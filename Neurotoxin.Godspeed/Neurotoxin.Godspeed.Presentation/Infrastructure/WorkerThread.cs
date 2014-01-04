using System;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public static class WorkerThread
    {
        public static void Run<T>(Func<T> work, Action<T> success = null, Action<Exception> error = null)
        {
            work.BeginInvoke(asyncResult =>
                {
                    try
                    {
                        var result = work.EndInvoke(asyncResult);
                        if (success != null) UIThread.Run(success, result);
                    }
                    catch (Exception ex)
                    {
                        if (error != null) UIThread.Run(error, ex);
                    }
                
            }, null);
        }
    }
}