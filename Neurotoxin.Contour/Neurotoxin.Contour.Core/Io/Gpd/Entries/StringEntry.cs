using System;
using System.Text;
using Neurotoxin.Contour.Core.Extensions;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Gpd.Entries
{
    public class StringEntry : EntryBase
    {
        public string Text
        {
            get { return ByteArrayExtensions.ToTrimmedString(AllBytes, Encoding.BigEndianUnicode); }
            set
            {
                var bytes = Encoding.BigEndianUnicode.GetBytes(value);
                Binary.WriteBytes(StartOffset, bytes, 0, bytes.Length);
            }
        }

        public StringEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}