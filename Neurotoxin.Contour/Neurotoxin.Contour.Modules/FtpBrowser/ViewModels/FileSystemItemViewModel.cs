using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FileSystemItemViewModel : ViewModelBase
    {
        private const string UPDIRECTORY = "[..]";

        private const string PATH = "Path";
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; NotifyPropertyChanged(PATH); }
        }

        private const string TITLE = "Title";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string TITLEID = "TitleId";
        private string _titleId;

        public string TitleId
        {
            get { return _titleId; }
            set { _titleId = value; NotifyPropertyChanged(TITLEID); }
        }

        private const string THUMBNAIL = "Thumbnail";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set { _thumbnail = value; NotifyPropertyChanged(THUMBNAIL); }
        }

        private const string TYPE = "Type";
        private ItemType _type;
        public ItemType Type
        {
            get { return _type; }
            set { _type = value; NotifyPropertyChanged(TYPE); }
        }

        private const string SUBTYPE = "SubType";
        private ItemSubtype _subType;
        public ItemSubtype Subtype
        {
            get { return _subType; }
            set { _subType = value; NotifyPropertyChanged(SUBTYPE); }
        }

        private const string SIZE = "Size";
        private long? _size;
        public long? Size
        {
            get { return _size; }
            set { _size = value; NotifyPropertyChanged(SIZE); }
        }

        private const string DATE = "Date";
        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; NotifyPropertyChanged(DATE); }
        }

        private const string ISSELECTED = "IsSelected";
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; NotifyPropertyChanged(ISSELECTED); }
        }

        public bool IsUpDirectory
        {
            get { return Title == UPDIRECTORY; }
            set { if (value) Title = UPDIRECTORY; }
        }

    }
}