using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;

namespace SliderLauncher
{
    public class IpAddressOrHostValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var input = value as string;

            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "IP address or hostname is required.");

            // Try IP address
            if (IPAddress.TryParse(input, out _))
                return ValidationResult.ValidResult;

            // Try DNS resolution
            try
            {
                var addresses = Dns.GetHostAddresses(input);
                if (addresses.Length > 0 && addresses.Any(a => a.AddressFamily == AddressFamily.InterNetwork || a.AddressFamily == AddressFamily.InterNetworkV6))
                    return ValidationResult.ValidResult;
            }
            catch
            {
                // DNS resolution failed
            }

            return new ValidationResult(false, "Invalid IP address or hostname.");
        }
    }
}
