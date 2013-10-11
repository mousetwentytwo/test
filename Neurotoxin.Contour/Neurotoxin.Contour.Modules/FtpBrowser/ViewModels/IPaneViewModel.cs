namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public interface IPaneViewModel
    {
        bool IsActive { get; }
        void SetActive();
        void Refresh();
    }
}