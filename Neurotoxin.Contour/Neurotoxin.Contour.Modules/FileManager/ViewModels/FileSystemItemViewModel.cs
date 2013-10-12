using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Neurotoxin.Contour.Core.Constants;
using Neurotoxin.Contour.Modules.FileManager.Constants;
using Neurotoxin.Contour.Modules.FileManager.Models;
using Neurotoxin.Contour.Presentation.Extensions;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
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

        public string Name
        {
            get { return _model.Name; }
        }

        public string ComputedName
        {
            get { return Title ?? Name; }
        }

        private const string THUMBNAIL = "THUMBNAIL";
        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    var bytes = _model.Thumbnail;
                    if (bytes == null)
                    {
                        switch (Type)
                        {
                            case ItemType.Directory:
                                bytes = ApplicationExtensions.GetContentByteArray("/Resources/folder.png");
                                break;
                            case ItemType.File:
                                bytes = ApplicationExtensions.GetContentByteArray("/Resources/file.png");
                                break;
                        }
                    }
                    _thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(bytes);
                }
                return _thumbnail;
            }
        }

        public ItemType Type
        {
            get { return _model.Type; }
        }

        private const string TITLETYPE = "TitleType";
        public TitleType TitleType
        {
            get { return _model.TitleType; }
            set { _model.TitleType = value; NotifyPropertyChanged(TITLETYPE); }
        }

        private const string CONTENTTYPE = "ContentType";
        public ContentType ContentType
        {
            get { return _model.ContentType; }
            set { _model.ContentType = value; NotifyPropertyChanged(CONTENTTYPE); }
        }

        private const string SIZE = "Size";
        public long? Size
        {
            get { return _model.Size; }
            set { _model.Size = value; NotifyPropertyChanged(SIZE); }
        }

        public long ComputedSize
        {
            get { return Size ?? 0; }
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
            NotifyPropertyChanged(TITLETYPE);
            NotifyPropertyChanged(CONTENTTYPE);
        }
    }
}