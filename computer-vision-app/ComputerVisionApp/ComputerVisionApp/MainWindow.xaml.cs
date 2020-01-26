using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;

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
            dialog.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            if (dialog.ShowDialog() == true)
            {
                this.ImagePath = dialog.FileName;
                this.ImageBitmap = new BitmapImage(new Uri(this.ImagePath));
                ImageContainer.Source = this.ImageBitmap;
            }
        }
    }
}
