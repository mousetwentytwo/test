using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Interfaces
{
    public interface IStoredConnection
    {
        string Name { get; set; }
        int ConnectionImage { get; set; }
    }
}