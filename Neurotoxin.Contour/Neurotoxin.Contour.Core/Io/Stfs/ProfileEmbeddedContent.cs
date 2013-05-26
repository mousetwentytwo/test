using System.Collections.Generic;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Io.Gpd;
using Neurotoxin.Contour.Core.Io.Stfs.Data;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs
{
    [DeclaredOnly]
    public class ProfileEmbeddedContent : StfsPackage
    {
        public override int HeaderSize
        {
            get { return 0x1000; }
            set {}
        }

        [BinaryData(0x228)]
        public override Certificate Certificate { get; set; }

        [BinaryData(0x14)]
        public override byte[] HeaderHash { get; set; }

        [BinaryData(0x8)]
        protected long Unknown1 { get; set; }

        [BinaryData(0x24)]
        public override StfsVolumeDescriptor VolumeDescriptor { get; set; }

        [BinaryData(0x4)]
        protected int Unknown2 { get; set; }

        [BinaryData(0x8)]
        public override byte[] ProfileId { get; set; }

        [BinaryData(0x1)]
        protected int Unknown3 { get; set; }

        [BinaryData(0x5)]
        public override byte[] ConsoleId { get; set; }

        public ProfileEmbeddedContent(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
        }

        public override void Resign(string kvPath = null)
        {
            ResignPackage(kvPath ?? "KV_dec.bin", 0x23C, 0xDC4, 0x23C);
        }

        protected override void ExtractGames()
        {
            Games = new Dictionary<FileEntry, GameFile>();
            foreach (var gpd in FileStructure.Files)
            {
                GetGameFile(gpd);
            }
        }

    }
}