using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pictagger.Models
{
    public class MappedImage
    {
        public static readonly int Resolution = 256;

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
            if(x >= 0 && x < Resolution && y >= 0 && y < Resolution)
                Map[y][x] = true;
        }

        public void Clear(int x, int y)
        {
            if (x >= 0 && x < Resolution && y >= 0 && y < Resolution)
                Map[y][x] = false;
        }

        public bool Get(int x, int y)
        {
            if (x >= 0 && x < Resolution && y >= 0 && y < Resolution)
                return Map[y][x];

            return false;
        }

        public void Fill(int x, int y)
        {
            if (x >= 0 && x < Resolution && y >= 0 && y < Resolution)
            {
                Map[y][x] = true;

                if (!Get(x, y - 1)) Fill(x, y - 1);
                if (!Get(x - 1, y)) Fill(x - 1, y);
                if (!Get(x, y + 1)) Fill(x, y + 1);
                if (!Get(x + 1, y)) Fill(x + 1, y);
            }
        }
    }
}
