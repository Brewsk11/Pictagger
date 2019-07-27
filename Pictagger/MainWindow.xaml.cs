using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pictagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Models.MappedImage CurrentMappedImage;
        public double BrushSize = 10.0;
        public Key eraseKey = Key.X;

        public string CurrentPath = "No path selected";

        public readonly int DefaultRes = 128;

        private Point PrevMousePos = new Point();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileNameLabel.Content = dlg.SafeFileName;
                
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dlg.FileName);
                bitmap.EndInit();

                image.Source = bitmap;
                image2.Source = bitmap;
                image3.Source = bitmap;
                image4.Source = bitmap;
                image5.Source = bitmap;

                CurrentMappedImage = new Models.MappedImage(DefaultRes);

                RefreshCanvas(canvas, CurrentMappedImage);
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Exit early if no image is loaded
            if (CurrentMappedImage == null) return;
            
            Point p = Mouse.GetPosition(canvas);
            PrevMousePos = p;
                
            var x = (int)(p.X / canvas.Width * CurrentMappedImage.Resolution);
            var y = (int)(p.Y / canvas.Height * CurrentMappedImage.Resolution);
            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DrawCircle(canvas, BrushSize, p.X, p.Y);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                CurrentMappedImage.FloodFill(x, y);
                RefreshCanvas(canvas, CurrentMappedImage);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Exit early if no image is loaded
            if (CurrentMappedImage == null) return;

            MapCanvas(canvas, CurrentMappedImage);
            //RefreshCanvas(canvas, CurrentMappedImage);
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Exit early if no image is loaded
            if (CurrentMappedImage == null) return;

            Point CurrentMousePos = Mouse.GetPosition(canvas);
            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DrawCircle(canvas, BrushSize, CurrentMousePos.X, CurrentMousePos.Y);

                ////
                // Interpolation

                double threshhold = BrushSize; // In pixels

                double differenceX = CurrentMousePos.X - PrevMousePos.X;
                double differenceY = CurrentMousePos.Y - PrevMousePos.Y;

                double distance = PythagorasCalcC(differenceX, differenceY);

                int steps = (int)(distance / threshhold);

                double stepX = differenceX / steps;
                double stepY = differenceY / steps;

                for (int i = 0; i < steps; i++)
                {
                    DrawCircle(
                        canvas,
                        BrushSize,
                        PrevMousePos.X + (i * stepX),
                        PrevMousePos.Y + (i * stepY)
                        );
                }

                PrevMousePos = CurrentMousePos;
            }
            
        }
        
        private void MapCanvas(Canvas canvas, Models.MappedImage mappedImage)
        {
            int res = DefaultRes;

            double pixelWidth = canvas.Width / res;
            double pixelHeight = canvas.Height / res;

            for (int i = 0; i < res; i++)
                for (int j = 0; j < res; j++)
                    mappedImage.Clear(i, j);

            foreach (Rectangle rect in canvas.Children)
            {
                var y = (int)(Canvas.GetTop(rect) / pixelHeight);
                var x = (int)(Canvas.GetLeft(rect) / pixelWidth);

                mappedImage.Set(x, y);
            }
        }

        private void DrawCircle(Canvas canvas, double radius, double x, double y)
        {
            int res = DefaultRes;

            double pixelWidth = canvas.Width / res;
            double pixelHeight = canvas.Height / res;

            double startX = 0.0, startY = 0.0;

            while (startX < x - radius) startX += pixelWidth;
            while (startY < y - radius) startY += pixelHeight;

            double currentX = startX, currentY = startY;

            while(currentY < y + radius)
            {
                while(currentX < x + radius)
                {
                    if (PythagorasCalcC(x - currentX, y - currentY) < radius)
                    {
                        DrawPixel(canvas, currentX, currentY);
                    }

                    currentX += pixelWidth;
                }

                currentX = startX;
                currentY += pixelHeight;
            }
        }

        private void DrawPixel(Canvas canvas, Point point, int resolution = 128)
        {
            DrawPixel(canvas, point.X, point.Y);
        }

        private void DrawPixel(Canvas canvas, double x, double y, int resolution = 128)
        {
            int res = resolution;

            var pixelX = (int)(x / canvas.Width * res);
            var pixelY = (int)(y / canvas.Height * res);

            DrawPixel(canvas, pixelX, pixelY);
        }

        private void DrawPixel(Canvas canvas, int x, int y, int resolution = 128)
        {
            bool eraseMode = Keyboard.IsKeyDown(eraseKey);

            int res = resolution;

            double pixelWidth = canvas.Width / res;
            double pixelHeight = canvas.Height / res;

            Rectangle toRemove = null;
            
            foreach (Rectangle r in canvas.Children)
            {
                if (Math.Abs(Canvas.GetTop(r) - pixelHeight * y) < pixelHeight / 2 &&
                    Math.Abs(Canvas.GetLeft(r) - pixelWidth * x) < pixelWidth / 2)
                {
                    if (!eraseMode)
                    {
                        return;
                    }
                    else
                    {
                        toRemove = r;
                        break;
                    }
                }
            }

            if (!eraseMode)
            {
                Rectangle rect = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Green),
                    Width = pixelWidth,
                    Height = pixelHeight,
                    Opacity = 0.85
                };

                Canvas.SetTop(rect, pixelHeight * y);
                Canvas.SetLeft(rect, pixelWidth * x);

                canvas.Children.Add(rect);
            }
            else
            {
                canvas.Children.Remove(toRemove);
            }
        }

        private void RefreshCanvas(Canvas canvas, Models.MappedImage mappedImage)
        {
            int res = mappedImage.Resolution;

            canvas.Children.Clear();
            for (int i = 0; i < res; i++)
            {
                for(int j = 0; j < res; j++)
                {
                    if (!mappedImage.Get(j, i))
                        continue;

                    DrawPixel(canvas, j, i, res);
                }
            }
        }

        private void RefreshAllCanvases()
        {
            RefreshCanvas(canvas, CurrentMappedImage);
            RefreshCanvas(canvas2, CurrentMappedImage.Downscale(1));
            RefreshCanvas(canvas3, CurrentMappedImage.Downscale(2));
            RefreshCanvas(canvas4, CurrentMappedImage.Downscale(3));
            RefreshCanvas(canvas5, CurrentMappedImage.Downscale(4));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrushSize = e.NewValue;
        }
        
        private double PythagorasCalcC(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2.0) + Math.Pow(b, 2.0));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RefreshAllCanvases();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                brushSizeSlider.Value++;

            else if (e.Delta < 0)
                brushSizeSlider.Value--;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {




            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if(saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Bitmap bm = CurrentMappedImage.ToBitmap();
                bm.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);

                bm = CurrentMappedImage.Downscale(1).ToBitmap();
                bm.Save(saveFileDialog.FileName + "128", System.Drawing.Imaging.ImageFormat.Bmp);
                bm = CurrentMappedImage.Downscale(2).ToBitmap();
                bm.Save(saveFileDialog.FileName + "64", System.Drawing.Imaging.ImageFormat.Bmp);
                bm = CurrentMappedImage.Downscale(3).ToBitmap();
                bm.Save(saveFileDialog.FileName + "32", System.Drawing.Imaging.ImageFormat.Bmp);
                bm = CurrentMappedImage.Downscale(4).ToBitmap();
                bm.Save(saveFileDialog.FileName + "16", System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }
    }
}
