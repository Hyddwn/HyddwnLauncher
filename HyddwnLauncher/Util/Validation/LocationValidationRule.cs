using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace HyddwnLauncher.Util.Validation
{
    public class LocationValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;

            if (string.IsNullOrWhiteSpace(str))
                return new ValidationResult(false, Properties.Resources.ValidationLocationIsRequired);

            if (!File.Exists(str))
                return new ValidationResult(false, Properties.Resources.ValidationFileDoesNotExist);

            return new ValidationResult(true, null);
        }
    }
}