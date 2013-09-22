using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Core.Io.Stfs.Data;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs
{
    public abstract class SvodPackage : Package<SvodVolumeDescriptor>
    {
        protected SvodPackage(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        protected override void Parse()
        {
        }

        public override void Rehash()
        {
            throw new NotImplementedException();
        }
    }
}