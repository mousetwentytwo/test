using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FtpBrowser.ViewModels
{
    public class FtpItem : ViewModelBase
    {
        private const string TITLE = "TITLE";
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(TITLE); }
        }

        private const string TITLEID = "TITLEID";
        private string _titleId;
        public string TitleId
        {
            get { return _titleId; }
            set { _titleId = value; NotifyPropertyChanged(TITLEID); }
        }
    }
}