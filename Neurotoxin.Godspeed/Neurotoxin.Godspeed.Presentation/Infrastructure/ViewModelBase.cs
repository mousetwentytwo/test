using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected readonly IUnityContainer container;
        protected readonly IEventAggregator eventAggregator;

        protected bool IsDisposed { get; private set; }

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