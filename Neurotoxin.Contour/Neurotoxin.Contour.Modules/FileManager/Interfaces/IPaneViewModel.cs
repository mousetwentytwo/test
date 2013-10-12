namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface IPaneViewModel
    {
        bool IsActive { get; }
        void SetActive();
        void Refresh();
    }
}