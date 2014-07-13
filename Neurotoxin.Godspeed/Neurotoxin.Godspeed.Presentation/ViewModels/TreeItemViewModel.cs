using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Microsoft.Practices.ObjectBuilder2;

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

        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string THUMBNAIL = "THUMBNAIL";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; NotifyPropertyChanged(THUMBNAIL); }
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
            set
            {
                _isSelected = value; 
                if (Children != null) Children.ForEach(c => c.IsSelected = value);
                NotifyPropertyChanged(ISSELECTED);
            }
        }

        public object Content { get; set; }

        public TreeItemViewModel()
        {
            Children = new ObservableCollection<TreeItemViewModel>();
        }
    }
}