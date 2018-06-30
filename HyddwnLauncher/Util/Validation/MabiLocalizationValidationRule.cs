using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;
using HyddwnLauncher.Extensibility;

namespace HyddwnLauncher.Util.Validation
{
    public class MabiLocalizationValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string localization))
                throw new ArgumentException("Argument must be of type 'System.String'");

            if (string.IsNullOrWhiteSpace(localization))
                return new ValidationResult(false, "The client localization must be set!");

            return ClientLocalization.GetLocalization(localization) == "?"
                ? new ValidationResult(false, "Unknown or invalid client localization!")
                : new ValidationResult(true, null);
        }
    }
}
