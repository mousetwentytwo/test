using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Shell.Interfaces
{
    public interface IDisposablePane
    {
        string CloseButtonText { get; }
        DelegateCommand<EventInformation<EventArgs>> CloseCommand { get; }
    }
}