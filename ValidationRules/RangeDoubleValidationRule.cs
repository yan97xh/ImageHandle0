using System.Globalization;
using System.Windows.Controls;

namespace ImageHandle.ValidationRules
{
    internal class RangeDoubleValidationRule : ValidationRule
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult(false, "值不能为空");

            if (!double.TryParse(value.ToString(), out double result))
                return new ValidationResult(false, "请输入有效的数字");

            if (result < MinValue || result > MaxValue)
                return new ValidationResult(false, $"值必须在 {MinValue} 和 {MaxValue} 之间");

            return ValidationResult.ValidResult;
        }
    }
}