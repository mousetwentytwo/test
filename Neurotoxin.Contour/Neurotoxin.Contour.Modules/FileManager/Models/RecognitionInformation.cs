using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Modules.FileManager.Constants;

namespace Neurotoxin.Contour.Modules.FileManager.Models
{
    public class RecognitionInformation
    {
        public string Pattern { get; private set; }
        public string Title { get; private set; }
        public TitleType Type { get; private set; }

        public RecognitionInformation(string pattern, string title, TitleType type = TitleType.Undefined)
        {
            Pattern = pattern;
            Title = title;
            Type = type;
        }
    }
}