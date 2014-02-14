using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Events;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class UserMessageViewModel : ViewModelBase
    {
        public string Message { get; private set; }

        private string _iconName;
        private ImageSource _icon;
        public ImageSource Icon
        {
            get
            {
                if (_icon == null)
                {
                    var png = ApplicationExtensions.GetContentByteArray(string.Format("/Resources/{0}.png", _iconName));
                    _icon = StfsPackageExtensions.GetBitmapFromByteArray(png);
                }
                return _icon;
            }
        }

        private const string ISREAD = "IsRead";
        private bool _isRead;
        public bool IsRead
        {
            get { return _isRead; }
            set { _isRead = value; NotifyPropertyChanged(ISREAD); }
        }

        public UserMessageViewModel(NotifyUserMessageEventArgs e)
        {
            Message = e.Message;
            _iconName = e.Icon;
        }
    }
}