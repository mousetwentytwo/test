using System;
using System.Diagnostics;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    public static class Web
    {
        public static bool Browse(string url)
        {
            try
            {
                Process.Start(url);
                return true;
            }
            catch (Exception ex)
            {
                NotificationMessage.ShowMessage(Resx.SystemError, string.Format(Resx.UrlCannotBeOpened, ex.Message));
                return false;
            }
        }
    }
}
