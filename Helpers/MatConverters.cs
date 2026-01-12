using OpenCvSharp;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageHandle.Helpers
{
    public static class MatConverters
    {
        // bitmap 转成 byte数组
        public static byte[] BitmapToByteArr(Bitmap _bitmap)
        {
            MemoryStream ms = new MemoryStream();
            _bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            return bytes;
        }

        // byte数组 转换成 bitmap
        public static Bitmap ByteArrToBitmap(byte[] _byteArr)
        {
            MemoryStream ms1 = new MemoryStream(_byteArr);
            Bitmap bm = (Bitmap)Image.FromStream(ms1);
            ms1.Close();
            return bm;
        }

        //  Mat 转换成 Bitmap
        public static BitmapSource MatToBitmap(OpenCvSharp.Mat _mat)
        {
            BitmapSource map = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_mat);
            return map;
        }

        // bitmap 转换成 mat
        public static Mat BitmapToMat(BitmapSource _bitmap)
        {
            // 使用这个时 会转换成 8UC4  然后画图就不好使了
            // return OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToMat(_bitmap);
            // cv2.line 等画图方法在 8UC3 工作好的 所以使用下面的方法
            //Mat dstImg = new Mat(_bitmap.PixelHeight, _bitmap.PixelWidth, MatType.CV_8UC3);
            //OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToMat(_bitmap, dstImg);

            using (Mat temp = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToMat(_bitmap))
            {
                return temp.Channels() == 3 ? temp.Clone() : temp.CvtColor(ColorConversionCodes.BGRA2BGR);
            }

            //  return dstImg;
        }

        // image 转换成 bitmap
        public static Bitmap ImageToBitmap(Image _image)
        {
            Bitmap bitmap = new Bitmap(_image);
            return bitmap;
        }

        // bitmap 转换成 image
        public static Image BitmapToImage(Bitmap _bitmap)
        {
            Image image = _bitmap;
            return image;
        }
    }
}