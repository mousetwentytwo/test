using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class FreestyleDatabaseCheckEvent : CompositePresentationEvent<FreestyleDatabaseCheckEventArgs> { }

    public class FreestyleDatabaseCheckEventArgs
    {
        public FtpContentViewModel FtpContentViewModel { get; private set; }

        public FreestyleDatabaseCheckEventArgs(FtpContentViewModel ftp)
        {
            FtpContentViewModel = ftp;
        }
    }
}