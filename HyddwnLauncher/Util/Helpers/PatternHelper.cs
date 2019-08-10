using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyddwnLauncher.Util.Helpers
{
    public class PatternHelper
    {
        public static string ConvertToPatternString(short[] array)
        {
            var stringBuilder = new StringBuilder();
            foreach (var element in array)
            {
                if (element == -1)
                {
                    stringBuilder.Append("?? ");
                    continue;
                }

                stringBuilder.AppendFormat("{0:X2} ", element);
            }

            return stringBuilder.ToString();
        }

        public static string ConvertToPatternString(byte[] array)
        {
            var stringBuilder = new StringBuilder();
            foreach (var element in array)
            {
                stringBuilder.AppendFormat("{0:X2} ", element);
            }

            return stringBuilder.ToString();
        }
    }
}
