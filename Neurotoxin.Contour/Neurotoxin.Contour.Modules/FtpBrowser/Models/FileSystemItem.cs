﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Models
{
    [Serializable]
    public class FileSystemItem
    {
        public string Title;
        public string TitleId;
        public byte[] Thumbnail;
        public ItemType Type;
        public ItemSubtype Subtype;
        [NonSerialized] public string Path;
        [NonSerialized] public long? Size;
        [NonSerialized] public DateTime Date;

        public FileSystemItem Clone()
        {
            return new FileSystemItem
                {
                    Title = Title,
                    TitleId = TitleId,
                    Thumbnail = Thumbnail,
                    Type = Type,
                    Subtype = Subtype,
                    Path = Path,
                    Size = Size,
                    Date = Date
                };
        }
    }
}