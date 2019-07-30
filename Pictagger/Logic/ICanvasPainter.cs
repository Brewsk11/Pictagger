using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Pictagger.Logic
{
    public enum PaintMode
    {
        Painter,
        Eraser
    }

    public interface ICanvasPainter
    {
        Canvas Canvas { get; }
        int Resolution { get; }

        double PixelWidth { get; }
        double PixelHeight { get; }

        void Brush(double x, double y, double radius, PaintMode mode);

        void DrawPixel(double x, double y);
        void DrawPixel(int x, int y);

        void RemovePixel(double x, double y);
        void RemovePixel(int x, int y);

        bool IsDrawn(int x, int y);
    }
}
