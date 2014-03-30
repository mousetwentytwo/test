using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class NoMessagesViewModel : ViewModelBase, IUserMessageViewModel
    {
        public string Message { get; private set; }
        public bool IsRead { get; set; }

        public NoMessagesViewModel()
        {
            Message = Resx.NoMessages;
            IsRead = true;
        }
    }
}