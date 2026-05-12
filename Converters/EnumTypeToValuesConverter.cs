using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImageHandle.Converters
{
    public class EnumTypeToValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type enumType)
            {
                // 处理可空枚举
                var actualEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
                if (actualEnumType.IsEnum)
                {
                    var enumValues = Enum.GetValues(actualEnumType);

                    // 如果是可空枚举，添加 null 选项
                    if (Nullable.GetUnderlyingType(enumType) != null)
                    {
                        var nullableValues = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
                        enumValues.CopyTo(nullableValues, 1);
                        return nullableValues;
                    }

                    return enumValues;
                }
            }
            return Array.Empty<object>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}