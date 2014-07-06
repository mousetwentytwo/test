using Neurotoxin.Godspeed.Core.Attributes;

namespace Neurotoxin.Godspeed.Shell.Constants
{
    public enum FtpServerType
    {
        Unknown,
        [StringValue("220 Minftpd ready")]
        MinFTPD,
        [StringValue("220 FSD FTPD ready")]
        FSD,
        [StringValue("220 F3 FTPD ready")]
        F3,
        [StringValue("220-XeXMenu FTPD 0.1, by XeDev")]
        XeXMenu,
        [StringValue("220-DLiFTPD 0.1")]
        DashLaunch,
        [StringValue("220-PS3 FTP Server")]
        PlayStation3,
        [StringValue("220 Microsoft FTP Service")]
        IIS,
        [StringValue("220 FtpDll Ready")]
        Aurora
    }
}