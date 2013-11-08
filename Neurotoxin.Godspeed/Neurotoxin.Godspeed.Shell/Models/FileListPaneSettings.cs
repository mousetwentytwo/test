using System;
using System.ComponentModel;

namespace Neurotoxin.Godspeed.Shell.Models
{
    [Serializable]
    public class FileListPaneSettings
    {
        public string Directory { get; set; }
        public string SortByField { get; set; }
        public ListSortDirection SortDirection { get; set; }

        public FileListPaneSettings(string directory, string sortByField, ListSortDirection sortDirection)
        {
            Directory = directory;
            SortByField = sortByField;
            SortDirection = sortDirection;
        }
    }
}