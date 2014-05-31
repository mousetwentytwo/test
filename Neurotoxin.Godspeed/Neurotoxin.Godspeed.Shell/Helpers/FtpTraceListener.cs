using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neurotoxin.Godspeed.Shell.Helpers
{
    public class FtpTraceListener : TraceListener
    {
        public readonly Stack<string> Log = new Stack<string>();

        public override void Write(string message)
        {
            lock (Log)
            {
                if (!message.EndsWith(Environment.NewLine)) message = Log.Pop() + message;
                Log.Push(message);
            }
        }

        public override void WriteLine(string message)
        {
            if (!message.EndsWith(Environment.NewLine)) message += Environment.NewLine;
            Write(message);
        }
    }
}