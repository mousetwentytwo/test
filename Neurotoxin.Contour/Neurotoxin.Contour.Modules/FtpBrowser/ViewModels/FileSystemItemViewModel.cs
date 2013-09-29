using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Neurotoxin.Contour.Modules.FtpBrowser.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FileSystemItemViewModel : ViewModelBase
    {
        private const string UPDIRECTORY = "[..]";

        private readonly FileSystemItem _model;
        public FileSystemItem Model
        {
            get { return _model; }
        }

        public string Path
        {
            get { return _model.Path; }
        }

        private const string TITLE = "Title";
        public string Title
        {
            get { return _model.Title; }
            set { _model.Title = value; NotifyPropertyChanged(TITLE); }
        }

        public string TitleId
        {
            get { return _model.TitleId; }
        }

        private const string THUMBNAIL = "THUMBNAIL";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail ?? (_thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(_model.Thumbnail)); }
        }

        public ItemType Type
        {
            get { return _model.Type; }
        }

        private const string SUBTYPE = "SubType";
        public ItemSubtype Subtype
        {
            get { return _model.Subtype; }
            set { _model.Subtype = value; NotifyPropertyChanged(SUBTYPE); }
        }

        private const string SIZE = "Size";
        public long? Size
        {
            get { return _model.Size; }
            set { _model.Size = value; NotifyPropertyChanged(SIZE); }
        }

        public DateTime Date
        {
            get { return _model.Date; }
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

        public FileSystemItemViewModel(FileSystemItem model)
        {
            _model = model;
        }

        public void NotifyModelChanges()
        {
            _thumbnail = null;
            NotifyPropertyChanged(TITLE);
            NotifyPropertyChanged(THUMBNAIL);
            NotifyPropertyChanged(SUBTYPE);
        }
    }
}