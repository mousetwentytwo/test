using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Neurotoxin.Contour.Modules.FtpBrowser.ViewModels;

namespace Neurotoxin.Contour.Modules.FtpBrowser.Helpers
{
    public class FileSystemItemViewModelComparer : IComparer<FileSystemItemViewModel>, IComparer
    {
        public string PropertyName { get; set; }
        public ListSortDirection SortDirection { get; set; }

        public int Compare(FileSystemItemViewModel x, FileSystemItemViewModel y)
        {
            if (x.IsUpDirectory) return -1;
            if (y.IsUpDirectory) return 1;
            int res;
            switch (PropertyName)
            {
                case "Title":
                    res = String.Compare(x.Title, y.Title, StringComparison.Ordinal);
                    break;
                case "Size":
                    res = !x.Size.HasValue ? -1 : x.Size.Value.CompareTo(y.Size);
                    break;
                case "Date":
                    res = x.Date.CompareTo(y.Date);
                    break;
                default:
                    throw new NotSupportedException("Invalid property:" + PropertyName);
            }
            return res * -1;
        }

        public int Compare(object x, object y)
        {
            return Compare(x as FileSystemItemViewModel, y as FileSystemItemViewModel);
        }
    }
}