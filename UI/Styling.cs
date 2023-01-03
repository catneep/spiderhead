using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Linq;
using spiderhead.Tools;

namespace spiderhead.UI
{
    class Styling
    {
        public static void ChangePrimaryColor(Dictionary<string, List<System.Windows.Media.GradientStop>> elements, Color color)
        {
            Color average = Colors.CalculateAverageColor(color);

            foreach (var element in elements["top"])
                element.Color = Colors.ToWinMediaColor(color);

            foreach (var element in elements["middle"])
                element.Color = Colors.ToWinMediaColor(average);
        }

        public static void ChangeBorderRadius(Border border, int radius = 8)
        {
            border.CornerRadius = new System.Windows.CornerRadius(radius);
        }
    }
}
