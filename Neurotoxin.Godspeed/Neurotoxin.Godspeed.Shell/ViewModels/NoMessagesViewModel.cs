using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Interfaces;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class NoMessagesViewModel : ViewModelBase, IUserMessageViewModel
    {
        public string Message { get; private set; }
        public bool IsRead { get; set; }

        public NoMessagesViewModel()
        {
            Message = "There are no messages yet.";
            IsRead = true;
        }
    }
}