using System;
using System.Collections.Generic;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Shell.ContentProviders;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string DISABLECUSTOMCHROME = "DisableCustomChrome";
        private bool _disableCustomChrome;
        public bool DisableCustomChrome
        {
            get { return _disableCustomChrome; }
            set { _disableCustomChrome = value; NotifyPropertyChanged(DISABLECUSTOMCHROME); }
        }

        private const string USEREMOTECOPY = "UseRemoteCopy";
        private bool _useRemoteCopy;
        public bool UseRemoteCopy
        {
            get { return _useRemoteCopy; }
            set { _useRemoteCopy = value; NotifyPropertyChanged(USEREMOTECOPY); }
        }

        public SettingsViewModel()
        {
            DisableCustomChrome = UserSettings.Get<bool>(UserSettings.DisableCustomChrome);
            UseRemoteCopy = UserSettings.Get<bool>(UserSettings.UseRemoteCopy);
        }

        public void SaveChanges()
        {
            UserSettings.Save(UserSettings.DisableCustomChrome, DisableCustomChrome);
            UserSettings.Save(UserSettings.UseRemoteCopy, UseRemoteCopy);
        }
    }
}