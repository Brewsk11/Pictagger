using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pictagger.Models
{
    public class MappedImage
    {
        public static readonly int Resolution = 128;

        private bool[][] Map;

        public MappedImage()
        {
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
    }
}
