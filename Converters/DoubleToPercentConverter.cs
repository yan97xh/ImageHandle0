using System.Globalization;
using System.Windows.Data;

namespace ImageHandle.Converters
{
    internal class DoubleToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double douleValue)
            {
                if (douleValue > 1)
                {
                    return douleValue.ToString("0.00");
                }
                return (douleValue * 100).ToString("0.00") + "%";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}