using Neurotoxin.Godspeed.Core.Constants;

namespace Neurotoxin.Godspeed.Core.Models
{
    public class BinaryContent
    {
        public string FilePath { get; private set; }
        public string TempFilePath { get; private set; }
        public byte[] Content { get; set; }
        public ContentType ContentType { get; set; }

        public BinaryContent(string filePath, string tempFilePath, byte[] content, ContentType contentType)
        {
            FilePath = filePath;
            TempFilePath = tempFilePath;
            Content = content;
            ContentType = contentType;
        }
    }
}