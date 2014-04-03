﻿using System;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    [Serializable]
    public class FileSystemItem
    {
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public byte[] Thumbnail { get; set; }
        public ItemType Type { get; set; }
        public TitleType TitleType { get; set; }
        public ContentType ContentType { get; set; }
        public RecognitionState RecognitionState { get; set; }

        public string Name;
        public string Path;
        public string FullPath;
        public long? Size;
        public DateTime Date;
        public bool IsLink;

        public bool IsCached;
        public bool IsLocked;
        public string LockMessage;

        private string _title;

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
                    Date = Date,
                    IsLink = IsLink
                };
        }

        public string GetRelativePath(string parent)
        {
            return string.IsNullOrEmpty(parent) ? Path : Path.Replace(parent, string.Empty);
        }
    }
}