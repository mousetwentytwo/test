using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Neurotoxin.Contour.Presentation.Validation
{
    public class IsRequired : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return new ValidationResult(value == null || value.ToString() != string.Empty, "You can't leave this field empty.");
        }
    }
}