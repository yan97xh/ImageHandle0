using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageHandle.Converters
{
    internal class StringToMatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowState windowState)
            {
                switch (windowState)
                {
                    case WindowState.Normal:
                        return "&#xe651;";
                    case WindowState.Maximized:
                        return "&#xe601;";
                    case WindowState.Minimized:
                        return "&#xe651;";
                    default:
                        return "&#xe651;";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
