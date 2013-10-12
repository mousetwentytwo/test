using Neurotoxin.Contour.Core.Attributes;

namespace Neurotoxin.Contour.Core.Constants
{
    public enum ContentType
    {
        Undefined,
        [StringValue("Saved Game")]
        SavedGame           = 0x00000001,
        [StringValue("Downloadable Content")]
        DownloadableContent = 0x00000002,
        [StringValue("Publisher")]
        Publisher           = 0x00000003,

        [StringValue("Xbox360 Title")]
        Xbox360Title        = 0x00001000,
        [StringValue("IPTV Pause Buffer")]
        IptvPauseBuffer     = 0x00002000,
        [StringValue("Installed Game")]
        InstalledGame       = 0x00004000,
        [StringValue("Xbox Original Game")]
        XboxOriginalGame    = 0x00005000,
        [StringValue("GOD Content")]
        GameOnDemand        = 0x00007000,
        [StringValue("Avatar Asset Pack")]
        AvatarAssetPack     = 0x00008000,
        [StringValue("Avatar Item")]
        AvatarItem          = 0x00009000,

        [StringValue("Profile Data")]
        Profile             = 0x00010000,
        [StringValue("Gamer Picture")]
        GamerPicture        = 0x00020000,
        [StringValue("Theme")]
        Theme               = 0x00030000,
        [StringValue("Cache File")]
        CacheFile           = 0x00040000,
        [StringValue("Storage Download")]
        StorageDownload     = 0x00050000,
        [StringValue("Xbox Saved Game")]
        XboxSavedGame       = 0x00060000,
        [StringValue("Xbox Download")]
        XboxDownload        = 0x00070000,
        [StringValue("Game Demo")]
        GameDemo            = 0x00080000,
        [StringValue("Video")]
        Video               = 0x00090000,
        [StringValue("XBLA Content")]
        XboxLiveArcadeGame  = 0x000D0000,
        [StringValue("Gamer Title")]
        GamerTitle          = 0x000A0000,
        [StringValue("Title Update")]
        TitleUpdate         = 0x000B0000,
        [StringValue("Game Trailer")]
        GameTrailer         = 0x000C0000,
        [StringValue("XNA Content")]
        XNA                 = 0x000E0000,
        [StringValue("License Store")]
        LicenseStore        = 0x000F0000,

        [StringValue("Movie")]
        Movie               = 0x00100000,
        [StringValue("TV Show")]
        Television          = 0x00200000,
        [StringValue("Musicvideo")]
        MusicVideo          = 0x00300000,
        [StringValue("Game Video")]
        GameVideo           = 0x00400000,
        [StringValue("Podcast Video")]
        PodcastVideo        = 0x00500000,
        [StringValue("Viral Video")]
        ViralVideo          = 0x00600000,
        [StringValue("Community Game")]
        CommunityGame       = 0x02000000,
    }
}