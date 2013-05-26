using System;
using Neurotoxin.Contour.Core.Attributes;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Core.Extensions;
using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Gpd.Entries
{
    public class TitleEntry : EntryBase
    {
        [BinaryData(4)]
        public virtual byte[] TitleId { get; set; }

        [BinaryData]
        public virtual int AchievementCount { get; set; }

        [BinaryData]
        public virtual int AchievementsUnlocked { get; set; }

        [BinaryData]
        public virtual int TotalGamerScore { get; set; }

        [BinaryData]
        public virtual int GamerscoreUnlocked { get; set; }

        [BinaryData]
        public virtual short AchievementsUnlockedOnline { get; set; }

        [BinaryData(1)]
        public virtual int AvatarAwardsEarned { get; set; }

        [BinaryData(1)]
        public virtual int AvatarAwardsCount { get; set; }

        [BinaryData(1)]
        public virtual int MaleAvatarAwardsEarned { get; set; }

        [BinaryData(1)]
        public virtual int MaleAvatarAwardsCount { get; set; }

        [BinaryData(1)]
        public virtual int FemaleAvatarAwardsEarned { get; set; }

        [BinaryData(1)]
        public virtual int FemaleAvatarAwardsCount { get; set; }

        [BinaryData(4)]
        public virtual TitleEntryFlags Flags { get; set; }

        [BinaryData]
        public virtual DateTime LastPlayed { get; set; }

        [BinaryData(StringReadOptions.NullTerminated)]
        public virtual string TitleName { get; set; }

        public string TitleCode { get; private set; }

        public DateTime LastAchievementEarnedOn { get; set; }

        public TitleEntry(OffsetTable offsetTable, BinaryContainer binary, int startOffset) : base(offsetTable, binary, startOffset)
        {
            TitleCode = TitleId.ToHex();
        }
    }
}