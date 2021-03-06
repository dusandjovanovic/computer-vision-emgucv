﻿using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ComputerVisionApp
{
    public partial class MainWindow : Window
    {
        private string ImagePath;
        private BitmapImage ImageBitmap;

        private double RectangleArea = 250;
        private long ColorRed = 0;
        private long ColorGreen = 0;
        private long ColorBlue = 0;

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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AreaTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RectangleArea = Double.Parse(AreaTextBox.Text);
        }

        private void ColorPicker_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (ColorPicker.SelectedColor.HasValue)
            {
                Color C = ColorPicker.SelectedColor.Value;
                ColorRed = Convert.ToInt64(C.R * (Math.Pow(256, 0)));
                ColorGreen = Convert.ToInt64(C.G * (Math.Pow(256, 0)));
                ColorBlue = Convert.ToInt64(C.B * (Math.Pow(256, 0)));
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
            this.ImageBitmap = EmguHelper.ImageDetectRectangles(this.ImageBitmap, this.RectangleArea, this.ColorRed, this.ColorGreen, this.ColorBlue);
            ImageContainer.Source = this.ImageBitmap;
        }
    }
}
