using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Util.Helpers
{
    internal static class MathHelper
    {
        public static double Normalize(double value, double minValue = 0, double maxValue = 100, double min = 0, double max = 1)
        {
            return (value - minValue) / (maxValue - minValue) * (max - min) + min;
        }
    }
}
