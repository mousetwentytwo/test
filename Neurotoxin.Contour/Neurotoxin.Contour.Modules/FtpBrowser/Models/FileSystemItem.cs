using System;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Modules.FtpBrowser.Constants;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    [Serializable]
    public class FileSystemItem
    {
        public string Title;
        public string Name;
        public byte[] Thumbnail;
        public ItemType Type;
        public TitleType TitleType;
        public ContentType ContentType;
        [NonSerialized] public string Path;
        [NonSerialized] public long? Size;
        [NonSerialized] public DateTime Date;

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
                    Size = Size,
                    Date = Date
                };
        }
    }
}