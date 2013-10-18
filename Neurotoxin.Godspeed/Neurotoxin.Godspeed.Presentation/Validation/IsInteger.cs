using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Validation
{
    public class IsInteger : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int res;
            if (value != null && Int32.TryParse(value.ToString(), out res))
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, string.Format("[{0}] is not an Integer value.", value));    
        }
    }
}