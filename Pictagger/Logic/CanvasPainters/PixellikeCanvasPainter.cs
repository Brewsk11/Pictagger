using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Pictagger.Logic
{
    public class PixellikeCanvasPainter : ICanvasPainter
    {
        public Canvas Canvas { get; }
        public int Resolution { get; }

        public double PixelWidth { get; }
        public double PixelHeight { get; }

        private Rectangle[][] _pixels;

        private readonly double drawnOpacity = 0.85;
        private readonly double erasedOpacity = 0.0;

        public PixellikeCanvasPainter(Canvas canvas, int resolution)
        {
            Canvas = canvas;
            Resolution = resolution;

            PixelWidth = Canvas.Width / Resolution;
            PixelHeight = Canvas.Height / Resolution;

            _pixels = new Rectangle[Resolution][];

            for(int y = 0; y < Resolution; y++)
            {
                _pixels[y] = new Rectangle[Resolution];

                for(int x = 0; x < Resolution; x++)
                {
                    _pixels[y][x] = new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.Green),
                        Width = PixelWidth,
                        Height = PixelHeight,
                        Opacity = drawnOpacity
                    };

                    Canvas.SetTop(GetPixel(x, y), PixelHeight * y);
                    Canvas.SetLeft(GetPixel(x, y), PixelWidth * x);

                    //Canvas.Children.Add(GetPixel(x, y));
                }
            }
        }

        private Rectangle GetPixel(int x, int y)
        {
            return _pixels[y][x];
        }

        public void Brush(double x, double y, double radius, PaintMode mode)
        {
            double startX = 0.0, startY = 0.0;

            while (startX < x - radius) startX += PixelWidth;
            while (startY < y - radius) startY += PixelHeight;

            double currentX = startX, currentY = startY;

            while (currentY < y + radius)
            {
                while (currentX < x + radius)
                {
                    if (MathUtils.Hypotenuse(x - currentX, y - currentY) < radius)
                    {
                        switch (mode)
                        {
                            case PaintMode.Painter:
                                DrawPixel(currentX, currentY);
                                break;

                            case PaintMode.Eraser:
                                RemovePixel(currentX, currentY);
                                break;

                            default:
                                break;
                        }
                    }

                    currentX += PixelWidth;
                }

                currentX = startX;
                currentY += PixelHeight;
            }
        }


        public void DrawPixel(double x, double y)
        {
            var pixels = CalcPixel(x, y);
            DrawPixel(pixels.Item1, pixels.Item2);
        }

        public void DrawPixel(int x, int y)
        {
            if (!IsInbound(x, y) || IsDrawn(x, y))
                return;

            Canvas.Children.Add(GetPixel(x, y));
        }

        public void RemovePixel(double x, double y)
        {
            var pixels = CalcPixel(x, y);
            RemovePixel(pixels.Item1, pixels.Item2);
        }

        public void RemovePixel(int x, int y)
        {
            if (!IsInbound(x, y) || !IsDrawn(x, y))
                return;

            Canvas.Children.Remove(GetPixel(x, y));
        }

        public Tuple<int, int> CalcPixel(double x, double y)
        {
            var pixelX = (int)(x / Canvas.Width * Resolution);
            var pixelY = (int)(y / Canvas.Height * Resolution);

            return new Tuple<int, int>(pixelX, pixelY);
        }

        public Tuple<double, double> CalcCoord(int x, int y)
        {
            var coordX = x * Canvas.Width / Resolution;
            var coordY = y * Canvas.Height / Resolution;

            return new Tuple<double, double>(coordX, coordY);
        }

        public bool IsDrawn(int x, int y)
        {
            if (!IsInbound(x, y)) return true;

            return Canvas.Children.Contains(GetPixel(x, y));
        }

        private bool IsInbound(int x, int y)
        {
            if (x >= 0 &&  x < Resolution &&
                y >= 0 && y < Resolution )
                return true;

            return false;
        }
    }
}
