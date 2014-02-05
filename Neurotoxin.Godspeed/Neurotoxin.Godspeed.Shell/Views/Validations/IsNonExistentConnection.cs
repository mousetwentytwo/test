using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using Neurotoxin.Godspeed.Core.Caching;
using Neurotoxin.Godspeed.Shell.ViewModels;

namespace Neurotoxin.Godspeed.Shell.Views.Validations
{
    public class IsNonExistentConnection : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(value == null || !EsentPersistentDictionary.Instance.ContainsKey(ConnectionsViewModel.CacheStoreKeyPrefix + value), "This connection name already exists");
        }
    }
}