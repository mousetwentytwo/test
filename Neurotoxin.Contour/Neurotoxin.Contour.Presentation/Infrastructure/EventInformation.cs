using System;

namespace Neurotoxin.Contour.Presentation.Infrastructure
{
    public class EventInformation<TEventArgsType> where TEventArgsType : EventArgs
    {
        public object Sender { get; set; }
        public TEventArgsType EventArgs { get; set; }
        public object CommandArgument { get; set; }
    }

}