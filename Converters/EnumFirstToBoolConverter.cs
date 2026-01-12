using System.Globalization;
using System.Windows.Data;

namespace ImageHandle.Converters
{
    internal class EnumFirstToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType().IsEnum == true)
            {
                Type enumType = value.GetType();
                var firstValue = Enum.GetValues(enumType).Cast<object>().First();
                return value.Equals(firstValue);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}