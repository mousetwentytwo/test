using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Events
{
    public class NotifyUserMessageEvent : CompositePresentationEvent<NotifyUserMessageEventArgs> { }

    public class NotifyUserMessageEventArgs
    {
        public string Message { get; private set; }
        public MessageIcon Icon { get; private set; }
        public MessageFlags Flags { get; private set; }
        public MessageCommand Command { get; private set; }
        public object CommandParameter { get; private set; }

        public NotifyUserMessageEventArgs(string message, MessageIcon icon, MessageCommand command = MessageCommand.DoNothing, object commandParameter = null, MessageFlags flags = MessageFlags.Ignorable)
        {
            Message = message;
            Icon = icon;
            Command = command;
            CommandParameter = commandParameter;
            Flags = flags;
        }
    }
}