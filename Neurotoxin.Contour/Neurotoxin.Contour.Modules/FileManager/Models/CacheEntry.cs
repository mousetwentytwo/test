using System;

namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    [Serializable]
    public class CacheEntry<T>
    {
        public DateTime? Expiration { get; set; }
        public DateTime? Date { get; set; }
        public long? Size { get; set; }
        public T Content { get; set; }
        public string TempFilePath { get; set; }
    }
}