using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HyddwnLauncher.Util.Validation
{
    public class LocationValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;

            if (string.IsNullOrWhiteSpace(str))
                return new ValidationResult(false, "Location is required!");

            if (!File.Exists(str))
                return new ValidationResult(false, "File does not exist!");

            return new ValidationResult(true, null);
        }
    }
}
