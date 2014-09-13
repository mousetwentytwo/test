using System;
using System.ComponentModel;
using Neurotoxin.Godspeed.Shell.Constants;

namespace Neurotoxin.Godspeed.Shell.Models
{
    [Serializable]
    public class FileListPaneSettings
    {
        public string Directory { get; set; }
        public string SortByField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public ColumnMode DisplayColumnMode { get; set; }

        public FileListPaneSettings(string directory, string sortByField, ListSortDirection sortDirection, ColumnMode columnMode)
        {
            Directory = directory;
            SortByField = sortByField;
            SortDirection = sortDirection;
            DisplayColumnMode = columnMode;
        }

        public FileListPaneSettings Clone(string directory = null)
        {
            return new FileListPaneSettings(directory ?? Directory, SortByField, SortDirection, DisplayColumnMode);
        }
    }
}