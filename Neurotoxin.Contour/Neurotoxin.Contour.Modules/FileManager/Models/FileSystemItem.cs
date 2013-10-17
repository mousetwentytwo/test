using System;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    [Serializable]
    public class FileSystemItem
    {
        public string Title { get; set; }
        public byte[] Thumbnail { get; set; }
        public ItemType Type { get; set; }
        public TitleType TitleType { get; set; }
        public ContentType ContentType { get; set; }

        public string Name;
        public string Path;
        public string FullPath;
        public long? Size;
        public DateTime Date;

        public FileSystemItem Clone()
        {
            return new FileSystemItem
                {
                    Title = Title,
                    Name = Name,
                    Thumbnail = Thumbnail,
                    Type = Type,
                    TitleType = TitleType,
                    ContentType = ContentType,
                    Path = Path,
                    FullPath = FullPath,
                    Size = Size,
                    Date = Date
                };
        }
    }
}