using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;

namespace ComputerVisionApp
{
    public static class EmguHelper
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        private static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static BitmapSource ToBitmapSource(Image image)
        {
            using (Bitmap source = new Bitmap(image))
            {
                IntPtr ptr = source.GetHbitmap();
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(ptr);
                return bs;
            }
        }

        public static BitmapImage ImageChangeBrightness(BitmapImage Image, double value)
        {
            Image<Bgr, Byte> TempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));
            TempImage._GammaCorrect(value);
            return Bitmap2BitmapImage(TempImage.Bitmap);
        }

        public static BitmapImage ImageChangeContrast(BitmapImage Image)
        {
            Image<Bgr, Byte> TempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));
            TempImage._EqualizeHist();
            return Bitmap2BitmapImage(TempImage.Bitmap);
        }

        public static BitmapImage ImageDetectContours(BitmapImage Image)
        {
            Image<Bgr, Byte> TempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));
            Image<Gray, byte> ImageOutput = TempImage.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat hier = new Mat();

            Image<Gray, byte> ImageReturn = new Image<Gray, byte>(TempImage.Width, TempImage.Height, new Gray(0));
            CvInvoke.FindContours(ImageOutput, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(ImageReturn, contours, -1, new MCvScalar(255, 0, 0));

            return Bitmap2BitmapImage(ImageReturn.Bitmap);
        }
    }
}
