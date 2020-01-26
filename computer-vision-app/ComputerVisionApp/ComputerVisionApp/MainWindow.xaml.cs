using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace ComputerVisionApp
{
    public partial class MainWindow : Window
    {
        private string ImagePath;
        private BitmapImage ImageBitmap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files (*.png)|*.PNG";
            if (dialog.ShowDialog() == true)
            {
                this.ImagePath = dialog.FileName;
                this.ImageBitmap = new BitmapImage(new Uri(this.ImagePath));
                ImageContainer.Source = this.ImageBitmap;
            }
        }

        private void BrightnessFilterIncrease(object sender, RoutedEventArgs e)
        {
            this.ImageBitmap = EmguHelper.ImageChangeBrightness(this.ImageBitmap, 0.8d);
            ImageContainer.Source = this.ImageBitmap;
        }

        private void BrightnessFilterDecrease(object sender, RoutedEventArgs e)
        {
            this.ImageBitmap = EmguHelper.ImageChangeBrightness(this.ImageBitmap, 1.2d);
            ImageContainer.Source = this.ImageBitmap;
        }

        private void ContrastFilter(object sender, RoutedEventArgs e)
        {
            this.ImageBitmap = EmguHelper.ImageChangeContrast(this.ImageBitmap);
            ImageContainer.Source = this.ImageBitmap;
        }

        private void ContureFilter(object sender, RoutedEventArgs e)
        {
            this.ImageBitmap = EmguHelper.ImageDetectContours(this.ImageBitmap);
            ImageContainer.Source = this.ImageBitmap;
        }

        private void DetectRectangles(object sender, RoutedEventArgs e)
        {

        }
    }
}
