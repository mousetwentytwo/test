﻿using System.Collections.Generic;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Appearance

        private const string DISABLECUSTOMCHROME = "DisableCustomChrome";
        private bool _disableCustomChrome;
        public bool DisableCustomChrome
        {
            get { return _disableCustomChrome; }
            set { _disableCustomChrome = value; NotifyPropertyChanged(DISABLECUSTOMCHROME); }
        }

        #endregion

        #region Operation

        private const string USEREMOTECOPY = "UseRemoteCopy";
        private bool _useRemoteCopy;
        public bool UseRemoteCopy
        {
            get { return _useRemoteCopy; }
            set { _useRemoteCopy = value; NotifyPropertyChanged(USEREMOTECOPY); }
        }

        #endregion

        #region Content recognition

        public List<int> ExpirationTimeSpans { get; set; }

        private const string USEJQE360 = "UseJqe360";
        private bool _useJqe360;
        public bool UseJqe360
        {
            get { return _useJqe360; }
            set { _useJqe360 = value; NotifyPropertyChanged(USEJQE360); }
        }

        private const string PROFILEEXPIRATION = "ProfileExpiration";
        private int _profileExpiration;
        public int ProfileExpiration
        {
            get { return _profileExpiration; }
            set { _profileExpiration = value; NotifyPropertyChanged(PROFILEEXPIRATION); }
        }

        private const string PROFILEINVALIDATION = "ProfileInvalidation";
        private bool _profileInvalidation;
        public bool ProfileInvalidation
        {
            get { return _profileInvalidation; }
            set { _profileInvalidation = value; NotifyPropertyChanged(PROFILEINVALIDATION); }
        }

        private const string RECOGNIZEDGAMEEXPIRATION = "RecognizedGameExpiration";
        private int _recognizedGameExpiration;
        public int RecognizedGameExpiration
        {
            get { return _recognizedGameExpiration; }
            set { _recognizedGameExpiration = value; NotifyPropertyChanged(RECOGNIZEDGAMEEXPIRATION); }
        }

        private const string PARTIALLYRECOGNIZEDGAMEEXPIRATION = "PartiallyRecognizedGameExpiration";
        private int _partiallyRecognizedGameExpiration;
        public int PartiallyRecognizedGameExpiration
        {
            get { return _partiallyRecognizedGameExpiration; }
            set { _partiallyRecognizedGameExpiration = value; NotifyPropertyChanged(PARTIALLYRECOGNIZEDGAMEEXPIRATION); }
        }

        private const string UNRECOGNIZEDGAMEEXPIRATION = "UnrecognizedGameExpiration";
        private int _unrecognizedGameExpiration;
        public int UnrecognizedGameExpiration
        {
            get { return _unrecognizedGameExpiration; }
            set { _unrecognizedGameExpiration = value; NotifyPropertyChanged(UNRECOGNIZEDGAMEEXPIRATION); }
        }

        private const string XBOXLIVECONTENTEXPIRATION = "XboxLiveContentExpiration";
        private int _xboxLiveContentExpiration;
        public int XboxLiveContentExpiration
        {
            get { return _xboxLiveContentExpiration; }
            set { _xboxLiveContentExpiration = value; NotifyPropertyChanged(XBOXLIVECONTENTEXPIRATION); }
        }

        private const string XBOXLIVECONTENTINVALIDATION = "XboxLiveContentInvalidation";
        private bool _xboxLiveContentInvalidation;
        public bool XboxLiveContentInvalidation
        {
            get { return _xboxLiveContentInvalidation; }
            set { _xboxLiveContentInvalidation = value; NotifyPropertyChanged(XBOXLIVECONTENTINVALIDATION); }
        }

        private const string UNKNOWNCONTENTEXPIRATION = "UnknownContentExpiration";
        private int _unknownContentExpiration;
        public int UnknownContentExpiration
        {
            get { return _unknownContentExpiration; }
            set { _unknownContentExpiration = value; NotifyPropertyChanged(UNKNOWNCONTENTEXPIRATION); }
        }

        #endregion

        public SettingsViewModel()
        {
            ExpirationTimeSpans = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 14, 21, 30, 60, 90};

            DisableCustomChrome = UserSettings.DisableCustomChrome;
            UseRemoteCopy = UserSettings.UseRemoteCopy;
            UseJqe360 = UserSettings.UseJqe360;
            ProfileExpiration = UserSettings.ProfileExpiration;
            ProfileInvalidation = UserSettings.ProfileInvalidation;
            RecognizedGameExpiration = UserSettings.RecognizedGameExpiration;
            PartiallyRecognizedGameExpiration = UserSettings.PartiallyRecognizedGameExpiration;
            UnrecognizedGameExpiration = UserSettings.UnrecognizedGameExpiration;
            XboxLiveContentExpiration = UserSettings.XboxLiveContentExpiration;
            XboxLiveContentInvalidation = UserSettings.XboxLiveContentInvalidation;
            UnknownContentExpiration = UserSettings.UnknownContentExpiration;
        }

        public void SaveChanges()
        {
            UserSettings.DisableCustomChrome = DisableCustomChrome;
            UserSettings.UseRemoteCopy = UseRemoteCopy;
            UserSettings.UseJqe360 = UseJqe360;
            UserSettings.ProfileExpiration = ProfileExpiration;
            UserSettings.ProfileInvalidation = ProfileInvalidation;
            UserSettings.RecognizedGameExpiration = RecognizedGameExpiration;
            UserSettings.PartiallyRecognizedGameExpiration = PartiallyRecognizedGameExpiration;
            UserSettings.UnrecognizedGameExpiration = UnrecognizedGameExpiration;
            UserSettings.XboxLiveContentExpiration = XboxLiveContentExpiration;
            UserSettings.XboxLiveContentInvalidation = XboxLiveContentInvalidation;
            UserSettings.UnknownContentExpiration = UnknownContentExpiration;
        }
    }
}