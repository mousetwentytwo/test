using System;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Gpd.Entries
{
    public class SyncData : EntryBase
    {
        [BinaryData]
        public virtual ulong NextSyncId { get; set; }

        [BinaryData]
        public virtual ulong LastSyncId { get; set; }

        [BinaryData]
        public virtual DateTime LastSyncedTime { get; set; }

        public SyncData(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}