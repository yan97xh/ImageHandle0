using System.Globalization;
using System.Windows.Controls;

namespace ImageHandle.ValidationRules
{
    public class DoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "值不能为空");

            if (double.TryParse(value.ToString(), out double result))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "请输入有效的数字");
        }
    }
}