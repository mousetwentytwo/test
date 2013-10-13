using System;
using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    [Serializable]
    public abstract class StoredConnectionBase
    {
        public string Name;
        public ConnectionImage ConnectionImage;
    }
}