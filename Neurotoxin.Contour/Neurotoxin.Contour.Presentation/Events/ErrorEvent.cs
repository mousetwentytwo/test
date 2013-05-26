using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Presentation.Events
{
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);

    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public ErrorEventArgs(string message)
        {
            this.Message = message;
        }
    }
}