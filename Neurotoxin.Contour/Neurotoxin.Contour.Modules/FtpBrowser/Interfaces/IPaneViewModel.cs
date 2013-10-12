namespace Neurotoxin.Contour.Modules.FtpBrowser.Interfaces
{
    public interface IPaneViewModel
    {
        bool IsActive { get; }
        void SetActive();
        void Refresh();
    }
}