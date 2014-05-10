using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Neurotoxin.Godspeed.Presentation.Extensions;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ContentProviders;
using Neurotoxin.Godspeed.Shell.Views.Dialogs;
using WPFLocalizeExtension.Engine;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly CacheManager _cacheManager;

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

        #region Operation

        public List<FsdContentScanTrigger> FsdContentScanTriggerOptions { get; set; }

        private const string USEVERSIONCHECKER = "UseVersionChecker";
        private bool _useVersionChecker;
        public bool UseVersionChecker
        {
            get { return _useVersionChecker; }
            set { _useVersionChecker = value; NotifyPropertyChanged(USEVERSIONCHECKER); }
        }

        private const string VERIFYFILEHASHAFTERFTPUPLOAD = "VerifyFileHashAfterFtpUpload";
        private bool _verifyFileHashAfterFtpUpload;
        public bool VerifyFileHashAfterFtpUpload
        {
            get { return _verifyFileHashAfterFtpUpload; }
            set { _verifyFileHashAfterFtpUpload = value; NotifyPropertyChanged(VERIFYFILEHASHAFTERFTPUPLOAD); }
        }

        private const string FSDCONTENTSCANTRIGGER = "FsdContentScanTrigger";
        private FsdContentScanTrigger _fsdContentScanTrigger;
        public FsdContentScanTrigger FsdContentScanTrigger
        {
            get { return _fsdContentScanTrigger; }
            set { _fsdContentScanTrigger = value; NotifyPropertyChanged(FSDCONTENTSCANTRIGGER); }
        }

        private const string USEREMOTECOPY = "UseRemoteCopy";
        private bool _useRemoteCopy;
        public bool UseRemoteCopy
        {
            get { return _useRemoteCopy; }
            set { _useRemoteCopy = value; NotifyPropertyChanged(USEREMOTECOPY); }
        }

        #endregion

        #region Appearance

        public List<CultureInfo> AvailableLanguages { get; set; }

        private const string LANGUAGE = "Language";
        private CultureInfo _language;
        public CultureInfo Language
        {
            get { return _language; }
            set { _language = value; NotifyPropertyChanged(LANGUAGE); }
        }

        private const string DISABLECUSTOMCHROME = "DisableCustomChrome";
        private bool _disableCustomChrome;
        public bool DisableCustomChrome
        {
            get { return _disableCustomChrome; }
            set { _disableCustomChrome = value; NotifyPropertyChanged(DISABLECUSTOMCHROME); }
        }

        #endregion

        #region ClearCacheCommand

        public DelegateCommand ClearCacheCommand { get; private set; }

        private void ExecuteClearCacheCommand()
        {
            WorkerThread.Run(() =>
                                 {
                                     UIThread.Run(() => NotificationMessage.ShowMessage(Resx.ApplicationIsBusy, Resx.PleaseWait, NotificationMessageFlags.NonClosable));
                                     _cacheManager.ClearCache();
                                     return true;
                                 }, 
                             r => NotificationMessage.CloseMessage());
        }

        #endregion

        public SettingsViewModel(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            ClearCacheCommand = new DelegateCommand(ExecuteClearCacheCommand);

            ExpirationTimeSpans = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 14, 21, 30, 60, 90 };
            FsdContentScanTriggerOptions = Enum.GetValues(typeof (FsdContentScanTrigger)).ToList<FsdContentScanTrigger>();
            AvailableLanguages = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c =>
            {
                try
                {
                    if (c.IsNeutralCulture) return false;
                    if (c.Equals(CultureInfo.InvariantCulture)) return false;
                    if (c.Name == "en-US") return true;
                    return Resx.ResourceManager.GetResourceSet(c, true, false) != null;
                }
                catch (CultureNotFoundException)
                {
                    return false;
                }
            }).ToList();

            UseJqe360 = UserSettings.UseJqe360;
            ProfileExpiration = UserSettings.ProfileExpiration;
            ProfileInvalidation = UserSettings.ProfileInvalidation;
            RecognizedGameExpiration = UserSettings.RecognizedGameExpiration;
            PartiallyRecognizedGameExpiration = UserSettings.PartiallyRecognizedGameExpiration;
            UnrecognizedGameExpiration = UserSettings.UnrecognizedGameExpiration;
            XboxLiveContentExpiration = UserSettings.XboxLiveContentExpiration;
            XboxLiveContentInvalidation = UserSettings.XboxLiveContentInvalidation;
            UnknownContentExpiration = UserSettings.UnknownContentExpiration;
            UseVersionChecker = UserSettings.UseVersionChecker;
            VerifyFileHashAfterFtpUpload = UserSettings.VerifyFileHashAfterFtpUpload;
            FsdContentScanTrigger = UserSettings.FsdContentScanTrigger;
            UseRemoteCopy = UserSettings.UseRemoteCopy;
            Language = UserSettings.Language ?? LocalizeDictionary.Instance.Culture;
            DisableCustomChrome = UserSettings.DisableCustomChrome;
        }

        public void SaveChanges()
        {
            UserSettings.UseJqe360 = UseJqe360;
            UserSettings.ProfileExpiration = ProfileExpiration;
            UserSettings.ProfileInvalidation = ProfileInvalidation;
            UserSettings.RecognizedGameExpiration = RecognizedGameExpiration;
            UserSettings.PartiallyRecognizedGameExpiration = PartiallyRecognizedGameExpiration;
            UserSettings.UnrecognizedGameExpiration = UnrecognizedGameExpiration;
            UserSettings.XboxLiveContentExpiration = XboxLiveContentExpiration;
            UserSettings.XboxLiveContentInvalidation = XboxLiveContentInvalidation;
            UserSettings.UnknownContentExpiration = UnknownContentExpiration;
            UserSettings.UseVersionChecker = UseVersionChecker;
            UserSettings.VerifyFileHashAfterFtpUpload = VerifyFileHashAfterFtpUpload;
            UserSettings.FsdContentScanTrigger = FsdContentScanTrigger;
            UserSettings.UseRemoteCopy = UseRemoteCopy;
            UserSettings.Language = Language;
            UserSettings.DisableCustomChrome = DisableCustomChrome;
        }
    }
}