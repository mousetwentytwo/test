using Microsoft.Practices.Composite.Presentation.Events;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class NotifyUserMessageEvent : CompositePresentationEvent<NotifyUserMessageEventArgs> { }

    public class NotifyUserMessageEventArgs
    {
        public string Message { get; private set; }
        public string Icon { get; private set; }
        //TODO: Dialog

        public NotifyUserMessageEventArgs(string message, string icon)
        {
            Message = message;
            Icon = icon;
        }
    }
}