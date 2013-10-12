using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace Neurotoxin.Contour.Presentation.Infrastructure
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
            MandatoryProperties = new Dictionary<string, MandatoryProperty>();
            foreach (var property in GetType().GetProperties())
            {
                var validators = (ValidationAttribute[]) property.GetCustomAttributes(typeof (ValidationAttribute), true);
                if (validators.Length == 0) continue;
                MandatoryProperties.Add(property.Name, new MandatoryProperty(property, validators));
            }
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

        private Dictionary<string, MandatoryProperty> MandatoryProperties { get; set; }

        private string Validate(string columnName)
        {
            if (MandatoryProperties.ContainsKey(columnName) && MandatoryProperties[columnName].IsEnabled)
            {
                var value = MandatoryProperties[columnName].Property.GetValue(this, null);
                var firstError = MandatoryProperties[columnName].Validators.FirstOrDefault(v => !v.IsValid(value));
                return firstError != null ? (firstError.ErrorMessage ?? "Invalid value") : null;
            }
            return null;
        }

        public bool HasErrors()
        {
            return MandatoryProperties.Keys.Any(HasError);
        }

        public bool HasError(string propertyName)
        {
            return string.IsNullOrEmpty(Validate(propertyName));
        }

    }
}