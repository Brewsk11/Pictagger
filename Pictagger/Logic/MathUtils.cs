using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pictagger.Logic
{
    public static class MathUtils
    {
        public static double Hypotenuse(double a, double b)
        {
            return Math.Sqrt(
                    Math.Pow(a, 2.0) + Math.Pow(b, 2.0)                
                );
        }
    }
}
