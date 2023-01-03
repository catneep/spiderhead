using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiderhead.Tools
{
    class Colors
    {
        public static Color CalculateAverageColor(Color colorA)
        {
            Color colorB = Color.FromArgb(222, 222, 222);
            return CalculateAverageColor(colorA, colorB);
        }

        public static Color CalculateAverageColor(Color colorA, Color colorB)
        {
            Func<byte, byte, int> getAverage = (byte a, byte b) => (int)Math.Floor((double)(a + b) / 2);

            Dictionary<char, int> argb = new();

            //argb.Add('A', getAverage(colorA.A, colorB.A));
            argb.Add('R', getAverage(colorA.R, colorB.R));
            argb.Add('G', getAverage(colorA.G, colorB.G));
            argb.Add('B', getAverage(colorA.B, colorB.B));

            return Color.FromArgb(argb['R'], argb['G'], argb['B']);
        }

        public static System.Windows.Media.Color ToWinMediaColor(Color c)
        {
            System.Windows.Media.Color color = new()
            {
                R = c.R,
                G = c.G,
                B = c.B
            };
            return color;
        }

    }
}
