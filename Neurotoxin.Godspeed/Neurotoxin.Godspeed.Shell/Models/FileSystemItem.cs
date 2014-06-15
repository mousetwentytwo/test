using System;
using System.Runtime.Serialization;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Core.Models;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class FileSystemItem : INamed
    {
        public string Title { get; set; }
        public byte[] Thumbnail { get; set; }
        public ItemType Type { get; set; }
        public TitleType TitleType { get; set; }
        public ContentType ContentType { get; set; }
        public RecognitionState RecognitionState { get; set; }

        [IgnoreDataMember]
        public string Name { get; set; }
        [IgnoreDataMember]
        public string Path { get; set; }
        [IgnoreDataMember]
        public string FullPath { get; set; }
        [IgnoreDataMember]
        public long? Size { get; set; }
        [IgnoreDataMember]
        public DateTime Date { get; set; }
        [IgnoreDataMember]
        public bool IsCached { get; set; }
        [IgnoreDataMember]
        public bool IsLocked { get; set; }
        [IgnoreDataMember]
        public string LockMessage { get; set; }

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

        public string GetRelativePath(string parent)
        {
            return string.IsNullOrEmpty(parent) ? Path : Path.Replace(parent, string.Empty);
        }
    }
}