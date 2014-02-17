using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public class ErrorReport : FormDataBase
    {
        public string ClientId { get; set; }
        public string ApplicationVersion { get; set; }
        public string FrameworkVersion { get; set; }
        public string OperatingSystemVersion { get; set; }
        public Exception Exception { get; set; }
        public Stack<string> FtpLog { get; set; }

        public override void Write(StreamWriter sw)
        {
            base.Write(sw);
            sw.WriteLine("Client ID: " + ClientId);
            sw.WriteLine("GODspeed version: " + ApplicationVersion);
            sw.WriteLine("Framework version: " + FrameworkVersion);
            sw.WriteLine("OS version: " + OperatingSystemVersion);
            sw.WriteLine();

            var ex = Exception;
            do
            {
                sw.WriteLine("Error: " + ex.Message);
                sw.WriteLine(ex.StackTrace);
                sw.WriteLine(String.Empty);
                ex = ex.InnerException;
            }
            while (ex != null);

            if (FtpLog == null) return;
            for (var i = FtpLog.Count - 1; i >= 0; i--)
            {
                sw.WriteLine(FtpLog.ElementAt(i));
            }
            sw.WriteLine();
        }
    }
}