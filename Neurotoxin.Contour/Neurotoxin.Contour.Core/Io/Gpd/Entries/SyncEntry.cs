using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Gpd.Entries
{
    public class SyncEntry : BinaryModelBase
    {
        public const int Size = 16;

        [BinaryData]
        public virtual ulong EntryId { get; set; }
        [BinaryData]
        public virtual ulong SyncId { get; set; }

        public SyncEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}