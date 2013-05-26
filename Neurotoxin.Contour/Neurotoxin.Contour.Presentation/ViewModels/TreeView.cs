using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Presentation.ViewModels
{
    public class TreeItem : ViewModelBase
    {
        private const string NAME = "Name";
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(NAME); }
        }

        private const string CHILDREN = "Children";
        private ObservableCollection<TreeItem> _children;
        public ObservableCollection<TreeItem> Children
        {
            get { return _children; }
            set { _children = value; NotifyPropertyChanged(CHILDREN); }
        }

        private const string ISDIRECTORY = "IsDirectory";
        private bool _isDirectory;
        public bool IsDirectory
        {
            get { return _isDirectory; }
            set { _isDirectory = value; NotifyPropertyChanged(ISDIRECTORY); }
        }
    }
}