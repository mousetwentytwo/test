using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Neurotoxin.Contour.Modules.FileManager.Events;
using Neurotoxin.Contour.Modules.FileManager.Interfaces;
using Neurotoxin.Contour.Presentation.Infrastructure;

namespace Neurotoxin.Contour.Modules.FileManager.ViewModels
{
    public abstract class PaneViewModelBase : ViewModelBase, IPaneViewModel
    {
        protected ModuleViewModelBase Parent { get; private set; }

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

        protected PaneViewModelBase(ModuleViewModelBase parent)
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

        public abstract void Refresh();
    }
}
