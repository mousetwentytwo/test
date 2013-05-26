using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs.Data
{
    public class PackageSignature : BinaryModelBase, IPackageSignature
    {
        [BinaryData(0x100)]
        public virtual byte[] Signature { get; set; }

        public PackageSignature(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}