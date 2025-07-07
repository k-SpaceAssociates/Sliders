using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClientTestApp
{
    public class PortValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "Port is required");

            if (int.TryParse(value.ToString(), out int port))
            {
                if (port >= 0 && port <= 65535)
                    return ValidationResult.ValidResult;

                return new ValidationResult(false, "Port must be between 0 and 65535");
            }

            return new ValidationResult(false, "Port must be an integer");
        }
    }
}
