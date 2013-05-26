using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Gpd
{
    public class XdbfFreeSpaceEntry : BinaryModelBase
    {
        [BinaryData]
        public virtual int AddressSpecifier { get; set; }

        [BinaryData]
        public virtual int Length { get; set; }

        public XdbfFreeSpaceEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}