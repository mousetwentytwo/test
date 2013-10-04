using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Limilabs.FTP.Client;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels.Helpers
{
    public class LocalWrapper : IFileManager
    {
        public List<FileSystemItem> GetList(string path = null)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FolderExists(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFolder(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void CreateFolder(string path)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadFileContent(string path)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadFileHeader(string path)
        {
            throw new NotImplementedException();
        }
    }
}
