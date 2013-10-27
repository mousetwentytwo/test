using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Neurotoxin.Godspeed.Shell.Events;
using Neurotoxin.Godspeed.Shell.Interfaces;
using Neurotoxin.Godspeed.Presentation.Infrastructure;
using Neurotoxin.Godspeed.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Godspeed.Shell.ViewModels
{
    public abstract class PaneViewModelBase : ViewModelBase, IPaneViewModel
    {
        protected FileManagerViewModel Parent { get; private set; }

        private const string ISACTIVE = "IsActive";
        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                var changed = value != _isActive;
                _isActive = value;
                NotifyPropertyChanged(ISACTIVE);
                if (value && changed) eventAggregator.GetEvent<ActivePaneChangedEvent>().Publish(new ActivePaneChangedEventArgs(this));
            }
        }

        private const string ISBUSY = "IsBusy";
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; NotifyPropertyChanged(ISBUSY); }
        }

        private const string PROGRESSMESSAGE = "ProgressMessage";
        private string _progressMessage;
        private int _progressPercentage;

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set { _progressMessage = value; NotifyPropertyChanged(PROGRESSMESSAGE); }
        }

        #region SetActiveCommand

        public DelegateCommand<EventInformation<MouseEventArgs>> SetActiveCommand { get; private set; }

        private void ExecuteSetActiveCommand(EventInformation<MouseEventArgs> eventInformation)
        {
            SetActive();
        }

        public virtual void SetActive()
        {
            IsActive = true;
        }

        #endregion

        protected PaneViewModelBase(FileManagerViewModel parent)
        {
            Parent = parent;
            SetActiveCommand = new DelegateCommand<EventInformation<MouseEventArgs>>(ExecuteSetActiveCommand);
            eventAggregator.GetEvent<ActivePaneChangedEvent>().Subscribe(OnActivePaneChanged);
        }

        protected virtual void OnActivePaneChanged(ActivePaneChangedEventArgs e)
        {
            if (e.ActivePane == this) return;
            IsActive = false;
        }

        public abstract void LoadDataAsync(LoadCommand cmd, object cmdParam, Action<PaneViewModelBase> success = null, Action<PaneViewModelBase, Exception> error = null);
    }
}