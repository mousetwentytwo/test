using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs.Data
{
    public class AvatarItemMediaInfo : BinaryModelBase, IMediaInfo
    {
        [BinaryData(EndianType.LittleEndian)]
        public virtual AssetSubcategory Subcategory { get; set; }

        [BinaryData(EndianType.LittleEndian)]
        public virtual int Colorizable { get; set; }

        [BinaryData(0x10)]
        public virtual byte[] GUID { get; set; }

        [BinaryData(0x1)]
        public virtual SkeletonVersion SkeletonVersion { get; set; }

        public AvatarItemMediaInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}