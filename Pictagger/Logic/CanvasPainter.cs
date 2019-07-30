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
    public class CanvasPainter
    {
        public Canvas Canvas { get; }
        public int Resolution { get; }

        public readonly double PixelWidth;
        public readonly double PixelHeight;

        public enum PaintMode
        {
            Painter,
            Eraser
        }

        public CanvasPainter(Canvas canvas, int resolution)
        {
            Canvas = canvas;
            Resolution = resolution;

            PixelWidth = Canvas.Width / Resolution;
            PixelHeight = Canvas.Height / Resolution;
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
            if (IsDrawn(x, y))
                return;
            
            Rectangle rect = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Green),
                Width = PixelWidth,
                Height = PixelHeight,
                Opacity = 0.85
            };

            Canvas.SetTop(rect, PixelHeight * y);
            Canvas.SetLeft(rect, PixelWidth * x);

            Canvas.Children.Add(rect);
        }

        private void RemovePixel(double x, double y)
        {
            var pixels = CalcPixel(x, y);
            RemovePixel(pixels.Item1, pixels.Item2);
        }

        private void RemovePixel(int x, int y)
        {
            if (!IsDrawn(x, y))
                return;
            
            foreach (Rectangle r in Canvas.Children)
            {
                if (Math.Abs(Canvas.GetTop(r) - PixelHeight * y) < PixelHeight / 2 &&
                    Math.Abs(Canvas.GetLeft(r) - PixelWidth * x) < PixelWidth / 2)
                {
                    Canvas.Children.Remove(r);
                    return;
                }
            }
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
            bool func(Rectangle rect)
            {
                if (Math.Abs(Canvas.GetTop(rect) - PixelHeight * y) < PixelHeight / 2 &&
                    Math.Abs(Canvas.GetLeft(rect) - PixelWidth * x) < PixelWidth / 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return Canvas.Children.OfType<Rectangle>().Any(func);
        }
    }
}
