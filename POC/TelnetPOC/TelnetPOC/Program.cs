using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using De.Mud.Telnet;
using Net.Graphite.Telnet;

namespace TelnetPOC
{
    class Program
    {
        private TelnetWrapper t;

        private void M()
        {
            t = new TelnetWrapper();
            t.Disconnected += OnDisconnect;
            t.DataAvailable += OnDataAvailable;
            t.TerminalType = "NETWORK-VIRTUAL-TERMINAL";
            t.Hostname = "pnakotus";
            t.Port = 23;
            t.Connect();
            t.Receive();

            t.Send(string.Concat("lftp", t.CR));
            t.Send(string.Concat("open 192.168.1.110", t.CR));
            t.Send(string.Concat("user xbox xbox", t.CR));
            t.Send(string.Concat("cd /Hdd1/Apps", t.CR));
            t.Send(string.Concat(@"put /mnt/HD_a2/x360/dlc/Nier\ -\ The\ World\ of\ Recycled\ Vessel\ DLC\ XBOX360/535107E8/00000002/B5849AE068830A162D28A72C2E07667A19BCD61853", t.CR));
            t.Send(string.Concat("exit", t.CR));
            t.Send(string.Concat("exit", t.CR));

            Console.ReadLine();
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            Console.WriteLine(string.Concat(Environment.NewLine, "Disconnected."));
            Console.WriteLine("Press [Enter] to quit.");
            Console.ReadLine();
        }

        private void OnDataAvailable(object sender, DataAvailableEventArgs e)
        {
            Console.Write(e.Data);
        }

        static void Main(string[] args)
        {
            //var p = new Program();
            //p.M();

            FileStream filestream = new FileStream("out.txt", FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            var telnet = new TcpClient("pnakotus", 23);
            var ns = telnet.GetStream();
            Console.WriteLine(Read(ns));
            Write(ns, "lftp");
            Console.WriteLine(Read(ns));
            Write(ns, "open 192.168.1.110");
            Console.WriteLine(Read(ns));
            Write(ns, "user xbox xbox");
            Console.WriteLine(Read(ns));
            Write(ns, "cd /Hdd1/Apps");
            Console.WriteLine(Read(ns));

            //Console.WriteLine("START");
            //Console.ReadLine();

            //Write(ns, @"put /mnt/HD_a2/x360/dlc/The.Darkness.II.DLC/5454088D/00000002/AB67BF2D3F996FB669BE1164FB42542986A818D254");
            Write(ns, @"put /mnt/HD_a2/x360/dlc/Nier\ -\ The\ World\ of\ Recycled\ Vessel\ DLC\ XBOX360/535107E8/00000002/B5849AE068830A162D28A72C2E07667A19BCD61853");
            //Console.WriteLine("FINISHED.");
            //Console.ReadLine();
            telnet.Close();
        }

        private static string Read(NetworkStream ns)
        {
            var sb = new StringBuilder();
            if (ns.CanRead)
            {
                var readBuffer = new byte[1024];
                while (ns.DataAvailable) 
                {
                    var numBytesRead = ns.Read(readBuffer, 0, readBuffer.Length);
                    var data = Encoding.ASCII.GetString(readBuffer, 0, numBytesRead);
                    data = data.Replace(Convert.ToChar(24), ' ');
                    data = data.Replace(Convert.ToChar(255), ' ');
                    data = data.Replace('?', ' ');
                    sb.AppendFormat("{0}", data);
                } 
            }
            Thread.Sleep(100);
            return sb.ToString();
        }

        private static void Write(NetworkStream ns, string message)
        {
            var msg = Encoding.ASCII.GetBytes(message + Environment.NewLine);
            ns.Write(msg, 0, msg.Length);
            string x;
            do
            {
                x = Read(ns);
                Console.WriteLine(x);
            } while (!x.EndsWith("> "));
        }
    }
}