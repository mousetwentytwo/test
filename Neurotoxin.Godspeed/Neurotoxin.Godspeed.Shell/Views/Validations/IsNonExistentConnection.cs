using System.Globalization;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Validations
{
    public class IsNonExistentConnection : ValidationRule
    {
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return string.IsNullOrEmpty(_errorMessage) ? "This connection name already exists." : _errorMessage; }
            set { _errorMessage = value; }
        }

        public string OriginalValue { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult((string)value == OriginalValue || !EsentPersistentDictionary.Instance.ContainsKey(ConnectionsViewModel.CacheStoreKeyPrefix + value), ErrorMessage);
        }
    }
}