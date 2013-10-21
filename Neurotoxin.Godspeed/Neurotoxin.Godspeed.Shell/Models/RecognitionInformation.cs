using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    public class RecognitionInformation
    {
        public string Pattern { get; private set; }
        public string Title { get; private set; }
        public TitleType Type { get; private set; }
        public bool IsDirectoryOnly { get; private set; }

        public RecognitionInformation(string pattern, string title, TitleType type = TitleType.Undefined, bool isDirectoryOnly = true)
        {
            Pattern = pattern;
            Title = title;
            Type = type;
            IsDirectoryOnly = isDirectoryOnly;
        }
    }
}