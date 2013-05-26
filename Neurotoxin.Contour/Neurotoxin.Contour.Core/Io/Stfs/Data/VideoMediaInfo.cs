using System;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs.Data
{
    public class VideoMediaInfo : BinaryModelBase, IMediaInfo
    {
        [BinaryData(0x10)]
        public byte[] SeriesId { get; set; }

        [BinaryData(0x10)]
        public byte[] SeasonId { get; set; }

        [BinaryData]
        public ushort SeasonNumber { get; set; }

        [BinaryData]
        public ushort EpisodeNumber { get; set; }

        public VideoMediaInfo(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }
    }
}