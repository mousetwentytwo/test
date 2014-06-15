using System;
using System.Collections.ObjectModel;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.ViewModels
{
    public class TreeItemViewModel : ViewModelBase
    {
        private const string NAME = "Name";
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(NAME); }
        }

        private const string CHILDREN = "Children";
        private ObservableCollection<TreeItemViewModel> _children;
        public ObservableCollection<TreeItemViewModel> Children
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

        private const string ISSELECTED = "IsSelected";
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(ISSELECTED); }
        }
    }
}