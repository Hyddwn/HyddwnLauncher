using System;
using System.Globalization;
using System.Windows.Controls;
using HyddwnLauncher.Extensibility;

namespace HyddwnLauncher.Util.Validation
{
    public class MabiLocalizationValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string localization))
                return new ValidationResult(false, Properties.Resources.ValidationValueMustBeString);

            if (string.IsNullOrWhiteSpace(localization))
                return new ValidationResult(false, Properties.Resources.ValidationValueMustNotBeNull);

            return ClientLocalization.GetLocalization(localization) == "?"
                ? new ValidationResult(false,
                    string.Format(Properties.Resources.ValidationInvalidLocalizationString, localization))
                : new ValidationResult(true, null);
        }
    }
}