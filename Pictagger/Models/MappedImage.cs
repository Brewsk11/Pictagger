using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Pictagger.Models
{
    public class MappedImage
    {
        public int Resolution = 128;

        private bool[][] Map;

        public MappedImage(int resolution)
        {
            Resolution = resolution;

            Map = new bool[Resolution][];
            Random r = new Random();
            for(int i = 0; i < Resolution; i++)
            {
                Map[i] = new bool[Resolution];

                for (int j = 0; j < Resolution; j++)
                {
                    Map[i][j] = false;
                }
            }
        }

        public void Set(int x, int y)
        {
            if(IsInbound(x, y))
                Map[y][x] = true;
        }

        public void Clear(int x, int y)
        {
            if (IsInbound(x, y))
                Map[y][x] = false;
        }

        public bool Get(int x, int y)
        {
            if (IsInbound(x, y))
                return Map[y][x];

            return false;
        }


        private class FillNode
        {
            public int x;
            public int y;

            public FillNode(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public void FloodFill(int x, int y)
        {
            if (!IsInbound(x, y))
                return;

            if (Get(x, y))
                return;

            Set(x, y);
            Queue<FillNode> queue = new Queue<FillNode>();
            queue.Enqueue(new FillNode(x, y));

            while (queue.Count > 0)
            {
                FillNode n = queue.Dequeue();

                if (!Get(n.x, n.y - 1) && IsInbound(n.x, n.y - 1))
                {
                    Set(n.x, n.y - 1);
                    queue.Enqueue(new FillNode(n.x, n.y - 1));
                }

                if (!Get(n.x - 1, n.y) && IsInbound(n.x - 1, n.y))
                {
                    Set(n.x - 1, n.y);
                    queue.Enqueue(new FillNode(n.x - 1, n.y));
                }

                if (!Get(n.x, n.y + 1) && IsInbound(n.x, n.y + 1))
                {
                    Set(n.x, n.y + 1);
                    queue.Enqueue(new FillNode(n.x, n.y + 1));
                }

                if (!Get(n.x + 1, n.y) && IsInbound(n.x + 1, n.y)) {
                    Set(n.x + 1, n.y);
                    queue.Enqueue(new FillNode(n.x + 1, n.y));
                }
            }
        }

        public bool IsInbound(int x, int y)
        {
            if (x >= 0 && y >= 0 && y < Resolution && x < Resolution)
                return true;
            else
                return false;
        }

        public MappedImage Downscale(int factor)
        {
            int scale = (int)Math.Pow(2.0, factor);

            MappedImage downscaled = new MappedImage(Resolution / scale);
            
            for(int y = 0; y < Resolution; y += scale)
            {
                for(int x = 0; x < Resolution; x += scale)
                {
                    int counter = 0;

                    for(int localY = y; localY < y + scale; localY++)
                    {
                        for(int localX = x; localX < x +scale; localX++)
                        {
                            if (Get(localX, localY)) counter++;
                        }
                    }

                    if (counter > scale * scale / 2) downscaled.Set(x / scale, y / scale);
                }
            }

            return downscaled;
        }

        public Bitmap ToBitmap()
        {
            Bitmap bm = new Bitmap(Resolution, Resolution, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int y = 0; y < Resolution; y++)
                for (int x = 0; x < Resolution; x++)
                    if (Get(x, y))
                        bm.SetPixel(x, y, Color.White);
                    else
                        bm.SetPixel(x, y, Color.Black);

            return bm;
        }

        public void SaveEncoded(string path, string name)
        {
            string fullPath = path;
            if (path.EndsWith("\\") || path.EndsWith("/"))
                fullPath += name;
            else
                fullPath += "\\" + name;

            using (System.IO.FileStream f = System.IO.File.Create(fullPath))
            {
                for (int y = 0; y < Resolution; y++)
                    for (int x = 0; x < Resolution; x += 8)
                        if (Get(x, y))
                            f.WriteByte(1);
                        else
                            f.WriteByte(0);
            }
        }
    }
}
