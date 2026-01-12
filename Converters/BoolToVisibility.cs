using System.Globalization;
using System.Windows.Data;

namespace ImageHandle.Converters
{
    internal class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bvalue)
            {
                return bvalue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}