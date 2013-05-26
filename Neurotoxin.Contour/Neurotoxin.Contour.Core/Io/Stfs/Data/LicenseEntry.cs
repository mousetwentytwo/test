using System;
using System.IO;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs.Data
{
    public class LicenseEntry : BinaryModelBase
    {
        [BinaryData]
        public virtual ulong Data { get; set; }

        public LicenseType Type
        {
            get
            {
                var type = (int)(Data >> 48);
                if (!Enum.IsDefined(typeof(LicenseType), type))
                    throw new InvalidDataException("STFS: Invalid license type " + type);
                return (LicenseType) type;
            }
        }

        [BinaryData]
        public virtual uint Bits { get; set; }

        [BinaryData]
        public virtual uint Flags { get; set; } //TODO?

        public LicenseEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}