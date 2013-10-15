using System;
using System.ComponentModel.DataAnnotations;

namespace Neurotoxin.Contour.Modules.FileManager.Database
{
    [Table("CacheEntry")]
    public class CacheEntry
    {
        [Key]
        [Column("CacheKey")]
        public string CacheKey { get; set; }

        [Column("Date")]
        public DateTime? Date { get; set; }

        [Column("Size")]
        public long? Size { get; set; }

        [Column("Expiration")]
        public DateTime? Expiration { get; set; }

        [Column("Content")]
        [MaxLength]
        public byte[] Content { get; set; }

        [Column("TempFilePath")]
        public string TempFilePath { get; set; }
    }
}