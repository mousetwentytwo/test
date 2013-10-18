using System;
using System.Globalization;
using System.Windows.Controls;

namespace Neurotoxin.Godspeed.Presentation.Validation
{
    public class IsIPv4Address : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            if (str == string.Empty)
            {
                return new ValidationResult(false, "Please enter an IP Address.");
            }

            if (str != null)
            {
                var parts = str.Split('.');
                if (parts.Length != 4)
                {
                    return new ValidationResult(false, "IP Address should be four octets, seperated by decimals.");
                }

                foreach (var p in parts)
                {
                    int intPart;
                    if (!int.TryParse(p, NumberStyles.Integer, cultureInfo.NumberFormat, out intPart))
                    {
                        return new ValidationResult(false, "Each octet of an IP Address should be a number.");
                    }

                    if (intPart < 0 || intPart > 255)
                    {
                        return new ValidationResult(false, "Each octet of an IP Address should be between 0 and 255.");
                    }
                }
            }

            return new ValidationResult(true, null);
        }
    }
}