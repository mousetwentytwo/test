using Microsoft.Practices.Composite.Presentation.Events;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Presentation.Events
{
    /// <summary>
    /// Event args for <see cref="RequestWindowCloseEvent"/>.
    /// </summary>
    public class RequestWindowCloseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestWindowCloseEventArgs"/> class.
        /// </summary>
        /// <param name="sender">The sender viewmodel.</param>
        /// <param name="dialogRes">The dialog result to be set when dialog window closes.</param>
        public RequestWindowCloseEventArgs(ViewModelBase sender, bool? dialogRes)
        {
            this.Sender = sender;
            this.DialogResult = dialogRes;
        }

        /// <summary>
        /// Gets the sender viewmodel.
        /// </summary>
        public ViewModelBase Sender { get; private set; }

        /// <summary>
        /// Gets dialog result to be set when dialog window closes.
        /// </summary>
        public bool? DialogResult { get; private set; }
    }

    /// <summary>
    /// Raised by the viewmodel when it wants to close it's view (window).
    /// </summary>
    public class RequestWindowCloseEvent : CompositePresentationEvent<RequestWindowCloseEventArgs> { }
}