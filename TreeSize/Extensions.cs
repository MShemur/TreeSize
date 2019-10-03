using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSize
{
   public static class Extensions
    {
        public static double BytesToKilobytes(this double bytes)
        {
            return bytes / 1024d;
        }

        public static double BytesToMegabytes(this double bytes)
        {
            return bytes / 1024d / 1024d;
        }

        public static double BytesToGigabytes(this double bytes)
        {
            return bytes / 1024d / 1024d/1024d;
        }
    }
}
