using System.Globalization;
using System.Windows.Controls;

namespace ImageHandle.ValidationRules
{
    public class IntValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return new ValidationResult(false, "值不能为空");

            if (int.TryParse(value.ToString(), out int result))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "请输入有效的数字");
        }
    }
}