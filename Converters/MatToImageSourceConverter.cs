using OpenCvSharp;
using System.Globalization;
using System.Windows.Data;

namespace ImageHandle.Converters
{
    public class MatToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Mat mat && !mat.Empty())
            {
                if (mat == null || mat.Empty())
                    return null;
                try
                {
                    // 根据Mat类型进行相应的转换
                    Mat imageToShow = mat;

                    // 如果是单通道灰度图，转换为BGR
                    if (mat.Channels() == 1)
                    {
                        imageToShow = new Mat();
                        Cv2.CvtColor(mat, imageToShow, ColorConversionCodes.GRAY2BGR);
                    }
                    // 如果是4通道（带Alpha），转换为BGR
                    else if (mat.Channels() == 4)
                    {
                        imageToShow = new Mat();
                        Cv2.CvtColor(mat, imageToShow, ColorConversionCodes.BGRA2BGR);
                    }

                    var bitmapSource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(imageToShow);

                    return bitmapSource;
                }
                catch (Exception ex)
                {
                    return null;
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
