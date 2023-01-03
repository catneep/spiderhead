using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace spiderhead.Tools
{
    class Images
    {
        /// <summary>
        /// Analyzes each pixel of a beatmap and
        /// determines the most used color
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="exclusions"></param>
        /// <returns></returns>
        private static int GetBitmapColorMode(Bitmap bm, List<int> exclusions = null)
        {
            //Trace.WriteLine("Getting colors with exclusions:");
            //foreach (var e in exclusions)
            //    Trace.WriteLine(e);

            var colorIncidence = new Dictionary<int, int>();
            for (var x = 0; x < bm.Size.Width; x++)
                for (var y = 0; y < bm.Size.Height; y++)
                {
                    var pixelColor = bm.GetPixel(x, y).ToArgb();
                    if (exclusions != null && exclusions.Contains(pixelColor))
                        continue;

                    if (colorIncidence.Keys.Contains(pixelColor))
                        colorIncidence[pixelColor]++;
                    else
                        colorIncidence.Add(pixelColor, 1);
                }

            var mostUsedColor = colorIncidence.OrderByDescending(entry => entry.Value).ToDictionary(x => x.Key, x => x.Value).First().Key;

            //return Color.FromArgb(mostUsedColor);
            return mostUsedColor;
        }

        public static Dictionary<int, int> GetBitmapDominantColorsByHSL(Bitmap bm)
        {
            //Dictionary<int, int> colorIncidence = new();
            Dictionary<int, int> colorHuesCount = new();

            int widthLowerLimit = (int)Math.Round(bm.Size.Width * 0.33, 0);
            int widthUpperLimit = widthLowerLimit * 2;
            
            int heightLowerLimit = (int)Math.Round(bm.Size.Height * 0.33, 0);
            int heightUpperLimit = heightLowerLimit * 2;

            for (var x = widthLowerLimit; x < widthUpperLimit; x++)
                for (var y = heightLowerLimit; y < heightUpperLimit; y++)
                {
                    var pixel = bm.GetPixel(x, y);
                    //var pixelColor = pixel.ToArgb();

                    // Ignore transparency
                    if (pixel.A == 0)
                        continue;

                    var pixelH = pixel.GetHue();
                    var pixelS = pixel.GetSaturation();
                    var pixelL = pixel.GetBrightness();

                    var hFloor = (int)Math.Floor(pixelH);

                    if (colorHuesCount.Keys.Contains(hFloor))
                        colorHuesCount[hFloor]++;
                    else
                        colorHuesCount.Add(hFloor, 1);
                }

            return colorHuesCount.OrderByDescending(entry => entry.Value).ToDictionary(x => x.Key, x => x.Value);
            //var mostUsedColor = colorHuesCount.OrderByDescending(entry => entry.Value).ToDictionary(x => x.Key, x => x.Value).First().Key;
        }

        public static List<int> GetDominantColorValues(BitmapImage bm, int length = 3)
        {
            return GetDominantColorValues(BitmapImageToBitmap(bm), 3);
        }

        public static List<int> GetDominantColorValues(Bitmap bm, int length = 3)
        {
            var colors = new List<int>();
            colors.Add(-1);
            colors.Add(-16777216);

            for (int i = 0; i < 3; i++)
                colors.Add(GetBitmapColorMode(bm, colors));

            colors.Remove(-1);
            colors.Remove(-16777216);

            Trace.WriteLine("Found colors:");
            foreach (var color in colors)
                Trace.WriteLine(color);

            return colors;
        }

        public static Color GetNearestColor(int inputColor)
        {
            return GetNearestColor(Color.FromArgb(inputColor));
        }

        public static Color GetNearestColor(Color inputColor)
        {
            var inputRed = Convert.ToDouble(inputColor.R);
            var inputGreen = Convert.ToDouble(inputColor.G);
            var inputBlue = Convert.ToDouble(inputColor.B);
            var colors = new List<Color>();
            foreach (var knownColor in Enum.GetValues(typeof(KnownColor)))
            {
                var color = Color.FromKnownColor((KnownColor)knownColor);
                if (!color.IsSystemColor)
                    colors.Add(color);
            }
            var nearestColor = Color.Empty;
            var distance = 500.0;
            foreach (var color in colors)
            {
                // Compute Euclidean distance between the two colors
                var testRed = Math.Pow(Convert.ToDouble(color.R) - inputRed, 2.0);
                var testGreen = Math.Pow(Convert.ToDouble(color.G) - inputGreen, 2.0);
                var testBlue = Math.Pow(Convert.ToDouble(color.B) - inputBlue, 2.0);
                var tempDistance = Math.Sqrt(testBlue + testGreen + testRed);
                if (tempDistance == 0.0)
                    return color;
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    nearestColor = color;
                }
            }
            return nearestColor;
        }

        public static BitmapImage BitmapImageFromByteArray(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static byte[] BitmapToByteArray(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bm)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bm));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
