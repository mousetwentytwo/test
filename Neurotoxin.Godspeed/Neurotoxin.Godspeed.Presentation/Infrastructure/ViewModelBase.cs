using System;
using System.ComponentModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public abstract class ViewModelBase : IViewModel
    {
        protected readonly IUnityContainer container;
        protected readonly IEventAggregator eventAggregator;

        public bool IsDisposed { get; private set; }

        private const string ISBUSY = "IsBusy";
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; NotifyPropertyChanged(ISBUSY); }
        }

        public ViewModelBase()
        {
            container = UnityInstance.Container;
            eventAggregator = container.Resolve<IEventAggregator>();
        }

        /// <summary>
        /// Forces to re-evaluate CanExecute on the commands 
        /// </summary>
        public virtual void RaiseCanExecuteChanges()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            RaiseCanExecuteChanges();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            IsDisposed = true;
        }

    }
}