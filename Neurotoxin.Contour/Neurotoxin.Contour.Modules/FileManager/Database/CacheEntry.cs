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

        [Column("Expiration")]
        public DateTime? Expiration { get; set; }

        [Column("Content")]
        [MaxLength]
        public byte[] Content { get; set; }

        [Column("AdditionalDataPath")]
        public string AdditionalDataPath { get; set; }
    }
}