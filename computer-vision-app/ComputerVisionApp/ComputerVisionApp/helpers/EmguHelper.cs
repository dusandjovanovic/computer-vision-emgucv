using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using Emgu.CV.Util;
using System.Collections.Generic;
using Emgu.CV.CvEnum;

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
            using (MemoryStream outStream = new MemoryStream())
            {
                bitmap.Save(outStream, ImageFormat.Png);
                outStream.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = outStream;
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
            Image<Bgr, Byte> tempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));
            tempImage._GammaCorrect(value);

            return Bitmap2BitmapImage(tempImage.Bitmap);
        }

        public static BitmapImage ImageChangeContrast(BitmapImage Image)
        {
            Image<Bgr, Byte> tempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));
            tempImage._EqualizeHist();

            return Bitmap2BitmapImage(tempImage.Bitmap);
        }

        public static BitmapImage ImageDetectContours(BitmapImage Image)
        {
            Image<Bgr, Byte> tempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));
            Image<Gray, byte> bwImage = tempImage.Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            Image<Gray, byte> returnImage = new Image<Gray, byte>(tempImage.Width, tempImage.Height, new Gray(0));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(bwImage, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            CvInvoke.DrawContours(returnImage, contours, -1, new MCvScalar(255, 0, 0));

            return Bitmap2BitmapImage(returnImage.Bitmap);
        }

        public static BitmapImage ImageDetectRectangles(BitmapImage Image, double area, long colorRed, long colorGreen, long colorBlue)
        {
            Image<Bgr, Byte> tempImage = new Image<Bgr, Byte>(BitmapImage2Bitmap(Image));

            #region color filtering
            Image<Bgr, Byte> coloredImage = tempImage.PyrDown().PyrUp();
            coloredImage._SmoothGaussian(3);
            Image<Gray, Byte> bwImage = coloredImage.InRange(new Bgr(colorBlue, colorGreen, colorRed), new Bgr(colorBlue, colorGreen, colorRed));
            #endregion

            List<RotatedRect> rectangleList = new List<RotatedRect>(); //a box is a rotated rectangle

            #region find rectangles by contoure detection
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(bwImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        double contourArea = CvInvoke.ContourArea(approxContour, false);
                        if (contourArea > area) // only consider contours with area greater than "area"
                        {
                            if (approxContour.Size == 4) // contour has 4 vertices.
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                       edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion

                                if (isRectangle) rectangleList.Add(CvInvoke.MinAreaRect(approxContour));
                            }
                        }
                    }
                }
            }
            #endregion

            #region draw rectangles on-top of source image
            Image<Bgr, Byte> triangleRectangleImage = tempImage.CopyBlank();
            foreach (RotatedRect box in rectangleList)
                triangleRectangleImage.Draw(box, new Bgr(Color.DarkMagenta), 2);
            #endregion

            return Bitmap2BitmapImage(triangleRectangleImage.Bitmap);
        }
    }
}
