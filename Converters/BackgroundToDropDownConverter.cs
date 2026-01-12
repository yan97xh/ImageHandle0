using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ImageHandle.Converters
{
    public class BackgroundToDropDownConverter : IValueConverter
    {
        public static BackgroundToDropDownConverter Instance = new BackgroundToDropDownConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Brush brush)
            {
                // 将背景色变亮作为下拉框背景
                if (brush is SolidColorBrush solidBrush)
                {
                    Color color = solidBrush.Color;
                    Color lighterColor = Color.FromArgb(255,
                        (byte)Math.Min(color.R + 40, 255),
                        (byte)Math.Min(color.G + 40, 255),
                        (byte)Math.Min(color.B + 40, 255));
                    return new SolidColorBrush(lighterColor);
                }
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}