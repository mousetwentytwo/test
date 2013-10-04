using System.Collections.Generic;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers
{
    public interface IFileManager
    {
        List<FileSystemItem> GetList(string path = null);

        bool FileExists(string path);
        bool FolderExists(string path);

        void DeleteFolder(string path);
        void DeleteFile(string path);

        void CreateFolder(string path);

        byte[] ReadFileContent(string path);
        byte[] ReadFileHeader(string path);
    }
}