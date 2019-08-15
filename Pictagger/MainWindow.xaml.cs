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
using System.IO;

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

        public DirectoryInfo CurrentDirectory = null;
        public DirectoryInfo TaggedDirectory = null;
        public FileInfo CurrentFile = null;

        public List<FileInfo> FilesLeft, FilesTagged;

        public readonly int DefaultRes = 128;

        private Point PrevMousePos = new Point();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                //InitialDirectory = "C:\\",
                InitialDirectory = @"C:\Users\Admin\Documents\Mateusz\Studia\Inżynierka\Program\asl-alphabet\asl_alphabet_train",
                Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadImage(dlg.FileName);
            }
        }

        private void LoadImage(string path)
        {
            fileNameLabel.Content = path;
            CurrentFile = new FileInfo(path);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            image.Source = bitmap;
            image2.Source = bitmap;
            image3.Source = bitmap;
            image4.Source = bitmap;
            image5.Source = bitmap;

            CurrentMappedImage = new Models.MappedImage(DefaultRes);

            RefreshAllCanvases();
        }

        private void RefreshTaggedNumbers()
        {
            numberLeft.Content = FilesLeft.Count;
            numberTagged.Content = FilesTagged.Count;
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

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrushSize = e.NewValue;
        }
        
        private double PythagorasCalcC(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2.0) + Math.Pow(b, 2.0));
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                brushSizeSlider.Value++;

            else if (e.Delta < 0)
                brushSizeSlider.Value--;
        }
        
        private void OpenDirectoryDialog(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = @"C:\Users\Admin\Documents\Mateusz\Studia\Inżynierka\Program\asl-alphabet\asl_alphabet_train";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CurrentDirectory = new DirectoryInfo(dlg.SelectedPath);
                FilesLeft = new List<FileInfo>();
                FilesTagged = new List<FileInfo>();

                Directory.CreateDirectory(CurrentDirectory.FullName + "\\Tagged");
                DirectoryInfo taggedDir = new DirectoryInfo(CurrentDirectory.FullName + "\\Tagged");

                foreach (FileInfo f in taggedDir.EnumerateFiles())
                    if(f.Extension == ".bmp" && f.Name.Contains("_128.bmp"))
                        FilesTagged.Add(f);

                foreach(FileInfo f in CurrentDirectory.EnumerateFiles())
                    if(f.Extension == ".jpg")
                        FilesLeft.Add(f);

                foreach(FileInfo tagged in FilesTagged)
                {
                    string stripped = StripResolution(StripExtension(tagged.Name));
                    foreach(FileInfo f in FilesLeft)
                    {
                        if(StripExtension(f.Name) == stripped)
                        {
                            FilesLeft.Remove(f);
                            break;
                        }
                    }
                }

                directoryLabel.Content = CurrentDirectory.FullName;
                RefreshTaggedNumbers();
            }
        }

        private void SaveAndLoadNext(object sender, RoutedEventArgs e)
        {
            if (CurrentDirectory == null)
            {
                System.Windows.MessageBox.Show(
                    "Please select a directory before you use this option",
                    "No directory selected",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (CurrentFile != null)
            {
                SaveBmps(CurrentDirectory.FullName + "\\Tagged\\" + CurrentFile.Name);

                FilesTagged.Add(CurrentFile);
                string stripped = StripExtension(CurrentFile.Name);

                foreach (FileInfo f in FilesLeft)
                {
                    if (StripExtension(f.Name) == stripped)
                    {
                        FilesLeft.Remove(f);
                        break;
                    }
                }
            }

            RefreshTaggedNumbers();

            if(FilesLeft.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "No photos in the directory left. Please choose another directory.",
                    "No photos left",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            CurrentFile = FilesLeft[new Random().Next(0, FilesLeft.Count)];
            LoadImage(CurrentFile.FullName);
        }

        private void SaveAsBitmapsWithDialog(object sender, RoutedEventArgs e)
        {
            if (CurrentMappedImage == null)
            {
                System.Windows.MessageBox.Show(
                    "Please load a file before you use this option",
                    "No file loaded",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);

                return;
            }

            FileInfo currFile = new FileInfo((string)fileNameLabel.Content);

            SaveFileDialog sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = "bmp",
                FileName = StripExtension(currFile.Name),
                Filter = "Image files (*.bmp)|*.bmp|All Files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveBmps(sfd.FileName);
            }
        }

        private void SaveBmps(string path)
        {
            FileInfo fi = new FileInfo(path);

            string nameNoExt = StripExtension(fi.Name);

            string baseName = fi.DirectoryName + "\\" + nameNoExt;

            System.Drawing.Bitmap bm = CurrentMappedImage.ToBitmap();
            bm.Save(baseName + "_128.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            bm = CurrentMappedImage.Downscale(1).ToBitmap();
            bm.Save(baseName + "_64.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            bm = CurrentMappedImage.Downscale(2).ToBitmap();
            bm.Save(baseName + "_32.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            bm = CurrentMappedImage.Downscale(3).ToBitmap();
            bm.Save(baseName + "_16.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            bm = CurrentMappedImage.Downscale(4).ToBitmap();
            bm.Save(baseName + "_8.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        private string StripResolution(string str)
        {
            string ret = str;
            ret = ret.Replace("_128", "");
            ret = ret.Replace("_64", "");
            ret = ret.Replace("_32", "");
            ret = ret.Replace("_16", "");
            ret = ret.Replace("_8", "");

            return ret;
        }

        private string StripExtension(string str)
        {
            string nameNoExt = "";
            
            string[] parts = str.Split('.');
            if (parts.Count() == 1)
                return parts[0];

            for (int i = 0; i < parts.Count() - 1; i++) nameNoExt += parts[i];
            return nameNoExt;
        }

        private void TransferToLowerRes(object sender, RoutedEventArgs e)
        {
            RefreshAllCanvases();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canvas == null)
                return;

            var item = (ComboBoxItem)e.AddedItems[0];

            switch(item.Content)
            {
                case "Pen":
                    canvas.Cursor = System.Windows.Input.Cursors.Pen;
                    break;

                case "Cross":
                    canvas.Cursor = System.Windows.Input.Cursors.Cross;
                    break;

                case "Arrow":
                    canvas.Cursor = System.Windows.Input.Cursors.Arrow;
                    break;

                default:
                    break;
            }
        }

        private void SkipPhoto(object sender, RoutedEventArgs e)
        {
            if (CurrentDirectory == null)
            {
                System.Windows.MessageBox.Show(
                    "Please select a directory before you use this option",
                    "No directory selected",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (FilesLeft.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    "No photos in the directory left. Please choose another directory.",
                    "No photos left",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            CurrentFile = FilesLeft[new Random().Next(0, FilesLeft.Count)];
            LoadImage(CurrentFile.FullName);
        }

        private void OpenGimp(object sender, RoutedEventArgs e)
        {
            if(CurrentFile == null)
            {
                System.Windows.MessageBox.Show(
                    "Select image.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            string gimpExe;
            if (File.Exists("config.txt"))
            {
                using (StreamReader configFile = new StreamReader("config.txt"))
                {
                    gimpExe = configFile.ReadLine();
                }
            }
            else
            {
                gimpExe = @"C:\Program Files\GIMP 2\bin\gimp-2.10.exe";
            }

            if(!File.Exists(gimpExe))
            {
                System.Windows.MessageBox.Show(
                    "Please, select gimp exacutable file.",
                    "Unable to find GIMP application.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                OpenFileDialog dlg = new OpenFileDialog
                {
                    InitialDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
                    Filter = "Application (*.exe)|*.exe",
                    RestoreDirectory = true,
                    Title = "Select gimp executable file"
                };

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    gimpExe = dlg.FileName;
                    using (StreamWriter config = new StreamWriter("config.txt"))
                    {
                        config.WriteLine(gimpExe);
                    }
                }
            }
            System.Diagnostics.Process.Start("\"" + gimpExe + "\"", "\"" + CurrentFile.FullName + "\"");
            //SkipPhoto(new object(), new RoutedEventArgs());
        }

        private void ScaleGimpFolder(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshAllCanvases()
        {
            RefreshCanvas(canvas, CurrentMappedImage);
            RefreshCanvas(canvas2, CurrentMappedImage.Downscale(1));
            RefreshCanvas(canvas3, CurrentMappedImage.Downscale(2));
            RefreshCanvas(canvas4, CurrentMappedImage.Downscale(3));
            RefreshCanvas(canvas5, CurrentMappedImage.Downscale(4));
        }
    }
}
