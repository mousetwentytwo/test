using Neurotoxin.Contour.Core.Attributes;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    public enum ItemSubtype 
    {
        Undefined,
        [StringValue("Content")]
        Content,
        [StringValue("Unknown Profile")]
        Profile,
        [StringValue("Unknown Game")]
        GameSaves,
        [StringValue("Games")]
        GamesFolder,
        [StringValue("Unknown Game")]
        Game,
        [StringValue("NXE Contents")]
        NXEFiles,
        [StringValue("GOD Contents")]
        GODFiles,
        [StringValue("XBLA Contents")]
        XboxLiveArcadeGame,
        [StringValue("Unknown DLC")]
        DownloadableContent,
        [StringValue("Downloadable Contents")]
        DownloadableContents,
        [StringValue("Unknown TU")]
        TitleUpdate,
        [StringValue("Title Updates")]
        TitleUpdates,
        [StringValue("Avatar Items")]
        AvatarItems,
        [StringValue("XNA Indie Player")]
        IndiePlayer,
        [StringValue("System Data")]
        SystemData,
        [StringValue("Gamer Pictures")]
        GamerPictures,
        [StringValue("Dashboard Themes")]
        Themes,
        Videos
    }
}