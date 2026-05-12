using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;

namespace ImageHandle.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        public static string GetDescription<T>(this T value) where T : Enum
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
    }

    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type? _enumType;

        public Type? EnumType
        {
            get => _enumType;
            set
            {
                if (value != _enumType)
                {
                    if (value != null)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;
                        if (!enumType.IsEnum)
                        {
                            throw new Exception("类型必须是枚举");
                        }
                    }
                    _enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension(Type enumType)
        {
            EnumType = enumType;
        }

        public EnumBindingSourceExtension()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_enumType == null)
            {
                throw new Exception("必须设置枚举类型");
            }
            var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            var enumValues = Enum.GetValues(actualEnumType);
            if (actualEnumType == _enumType)
            {
                return enumValues;
            }
            var nullableEnumValues = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(nullableEnumValues, 1);
            return nullableEnumValues;
        }
    }
}