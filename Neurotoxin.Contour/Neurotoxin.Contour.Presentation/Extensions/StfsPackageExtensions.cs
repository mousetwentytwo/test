using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Neurotoxin.Contour.Core.Io.Stfs;
using Neurotoxin.Contour.Core.Io.Stfs.Data;
using Neurotoxin.Contour.Presentation.ViewModels;

namespace Neurotoxin.Contour.Presentation.Extensions
{
    public static class StfsPackageExtensions
    {
        public static ObservableCollection<TreeItem> BuildTreeFromFileListing(this StfsPackage package)
        {
            return new ObservableCollection<TreeItem> {BuildTree(package.FileStructure)};
        }

        public static BitmapImage GetThumbnailImage(this StfsPackage package)
        {
            return GetBitmapFromByteArray(package.ThumbnailImage);
        }

        public static BitmapImage GetTitleThumbnailImage(this StfsPackage package)
        {
            return GetBitmapFromByteArray(package.TitleThumbnailImage);
        }

        private static TreeItem BuildTree(FileEntry parent)
        {
            var treeItem = new TreeItem
                               {
                                   Name = parent.Name, 
                                   Children = new ObservableCollection<TreeItem>(),
                                   IsDirectory = parent.IsDirectory
                               };
            foreach (var folder in parent.Folders)
            {
                treeItem.Children.Add(BuildTree(folder));
            }
            foreach (var file in parent.Files)
            {
                treeItem.Children.Add(new TreeItem { Name = file.Name });
            }
            return treeItem;
        }

        public static BitmapImage GetBitmapFromByteArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(bytes);
            bitmap.EndInit();
            return bitmap;
        }


    }
}