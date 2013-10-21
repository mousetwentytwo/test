using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class ApplicationExtensions
    {
        private static readonly Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();

        public static byte[] GetContentByteArray(string path)
        {
            if (Cache.ContainsKey(path)) return Cache[path];

            var contentInfo = Application.GetResourceStream(new Uri(path, UriKind.Relative));
            if (contentInfo == null) throw new ArgumentException(string.Format("Content not found: {0}", path));
            var ms = new MemoryStream();
            contentInfo.Stream.CopyTo(ms);
            var bytes = ms.ToArray();
            Cache.Add(path, bytes);
            return bytes;
        }
    }
}