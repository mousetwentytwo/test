using Neurotoxin.Contour.Core.Models;

namespace Neurotoxin.Contour.Core.Io.Stfs.Data
{
    public interface IPackageSignature : IBinaryModel
    {
        byte[] Signature { get; set; }
    }
}