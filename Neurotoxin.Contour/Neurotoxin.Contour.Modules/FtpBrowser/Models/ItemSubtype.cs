using Neurotoxin.Contour.Core.Attributes;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    public enum ItemSubtype 
    {
        Undefined,

        #region Collection types

        //Content
        [StringValue("Content")]
        Content,

        //0000000000000000
        [StringValue("Games")]
        GamesFolder,

        //E00001*
        [StringValue("Unknown Profile")]
        Profile,

        //00000001
        [StringValue("Save Data")]
        SaveData,

        //00000002
        [StringValue("Downloadable Contents")]
        DownloadableContents,

        //00004000
        [StringValue("NXE Data")]
        NXEData,

        //00007000
        [StringValue("GOD Data")]
        GODData,

        //00009000
        [StringValue("Avatar Items")]
        AvatarItems,

        //00010000
        [StringValue("Profile Data")]
        ProfileData,

        //00020000
        [StringValue("Gamer Pictures")]
        GamerPictures,

        //00030000
        [StringValue("Dashboard Themes")]
        Themes,

        //00080000 && 000D0000
        [StringValue("XBLA Data")]
        XboxLiveArcadeGame,

        //00090000
        [StringValue("Videos")]
        Videos,

        //000B0000
        [StringValue("Title Updates")]
        TitleUpdates,

        //584E07D2
        [StringValue("XNA Indie Player")]
        IndiePlayer,

        //FFFE*
        [StringValue("System Data")]
        SystemData,

        #endregion

        #region Item types

        [StringValue("Unknown Game")]
        Game,
        [StringValue("Unknown Game")]
        GameSaves,
        [StringValue("Unknown DLC")]
        DownloadableContent,
        [StringValue("Unknown TU")]
        TitleUpdate

        #endregion

    }
}