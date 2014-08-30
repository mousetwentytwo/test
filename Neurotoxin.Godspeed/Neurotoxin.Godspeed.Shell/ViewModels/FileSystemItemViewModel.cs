using System;
using System.Windows.Media;
using Neurotoxin.Godspeed.Core.Constants;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.Models;
using Neurotoxin.Godspeed.Presentation.Extensions;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class FileSystemItemViewModel : CommonViewModelBase
    {
        private readonly FileSystemItem _model;
        public FileSystemItem Model
        {
            get { return _model; }
        }

        public string Path
        {
            get { return _model.Path; }
        }

        public string FullPath
        {
            get { return _model.FullPath; }
        }

        internal const string TITLE = "Title";
        public string Title
        {
            get { return _model.Title; }
            set { _model.Title = value; NotifyPropertyChanged(TITLE); }
        }

        internal const string NAME = "Name";
        public string Name
        {
            get { return _model.Name; }
            set { _model.Name = value; NotifyPropertyChanged(NAME); }
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
                                var dirIconPath = IsRefreshing ? "/Resources/refresh_folder.png" : "/Resources/folder.png";
                                bytes = ResourceManager.GetContentByteArray(dirIconPath);
                                break;
                            case ItemType.Link:
                                bytes = ResourceManager.GetContentByteArray("/Resources/reparse_point.png");
                                break;
                            case ItemType.File:
                                var fileIconPath = IsCompressedFile
                                    ? "/Resources/package.png"
                                    : IsXex ? "/Resources/xex.png" : "/Resources/file.png";
                                bytes = ResourceManager.GetContentByteArray(fileIconPath);
                                break;
                        }
                    }
                    _thumbnail = StfsPackageExtensions.GetBitmapFromByteArray(bytes);
                }
                return _thumbnail;
            }
        }

        public bool HasThumbnail
        {
            get { return _model.Thumbnail != null && !IsUpDirectory; }
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
            get { return Name == Strings.UpDirectory; }
            set { if (value) Name = Strings.UpDirectory; }
        }

        public bool IsCached
        {
            get { return _model.IsCached; }
        }

        public bool IsLocked
        {
            get { return _model.IsLocked; }
        }

        public string LockMessage
        {
            get { return _model.LockMessage; }
        }

        private const string ISREFRESHING = "IsRefreshing";
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set 
            { 
                _isRefreshing = value;
                _thumbnail = null;
                NotifyPropertyChanged(THUMBNAIL);
                NotifyPropertyChanged(ISREFRESHING);
            }
        }

        private const string ISGAME = "IsGame";
        public bool IsGame
        {
            get { return TitleType == TitleType.Game; }
        }

        private const string ISPROFILE = "IsProfile";
        public bool IsProfile
        {
            get { return TitleType == TitleType.Profile; }
        }

        private const string ISXEX = "IsXex";
        public bool IsXex
        {
            get
            {
                var ext = System.IO.Path.GetExtension(Name);
                return ext != null && ext.ToLower() == ".xex";
            }
        }

        public bool IsCompressedFile
        {
            get
            {
                var ext = System.IO.Path.GetExtension(Path).ToLower();
                return (ext == ".zip" || ext == ".rar" || ext == ".tar" || ext == ".tar.gz" || ext == ".7z");
            }
        }

        private static string CONTENT = "Content";
        private object _content;
        public object Content
        {
            get { return _content; }
            set { _content = value; NotifyPropertyChanged(CONTENT); }
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
            NotifyPropertyChanged(ISGAME);
            NotifyPropertyChanged(ISPROFILE);
            NotifyPropertyChanged(ISXEX);
            NotifyPropertyChanged(SIZE);
        }
    }
}