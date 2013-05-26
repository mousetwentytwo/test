using System;
using System.Collections.Generic;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Gpd.Entries
{
    public class SyncList : List<SyncEntry>
    {
        public XdbfEntry Entry { get; set; }

        public BinaryContainer Binary { get; set; }

        public byte[] AllBytes
        {
            get { return Binary.ReadAll(); }
        }
    }

}