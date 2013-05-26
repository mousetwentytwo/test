using System;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs.Data
{
    public class Update : BinaryModelBase, IInstallerInformation
    {
        [BinaryData(4)]
        public virtual Version BaseVersion { get; set; }

        [BinaryData(4)]
        public virtual Version Version { get; set; }

        public Update(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}