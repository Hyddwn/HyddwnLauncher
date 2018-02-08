using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility;
using HyddwnLauncher.Model;

namespace HyddwnLauncher.Util.Validation
{
    public class LocalizationValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string localization)) return new ValidationResult(false, "Value must be of stypre 'String'.");

            if (string.IsNullOrWhiteSpace(localization))
                return new ValidationResult(false, "Value must not be null.");

            return ClientLocalization.GetLocalization(localization) == "?" 
                ? new ValidationResult(false, $"'{localization}' is not a supported localization string!") 
                : new ValidationResult(true, null);
        }
    }
}
