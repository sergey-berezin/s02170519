using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Contracts
{
    public class Utils
    {
        public static BitmapSource LoadImage(byte[] imageBytes)
        {
            BitmapImage bmpImage = new BitmapImage();
            MemoryStream mystream = new MemoryStream(imageBytes);
            bmpImage.BeginInit();
            bmpImage.StreamSource = mystream;
            bmpImage.EndInit();
            bmpImage.Freeze();
            return bmpImage;
        }
        public static BitmapSource FromString(string data)
        {
            return ConvertToBitmapSource(BitmapFromBytes(Convert.FromBase64String(data)));
        }
        public static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        public static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            if (bmp == null)
                return null;
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }

        public static Bitmap BitmapFromBytes(byte[] bytes)
        {
            Bitmap bmp;
            try
            {
                bmp = new Bitmap(new MemoryStream(bytes));
            }
            catch
            {
                bmp = null;
            }
            return ResizeBitmap(bmp, 28, 28);
        }
    }
}
