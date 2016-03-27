using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ImageDeduplicator.Identifiers {
    class HistogramIdentifier : AIdentifier {
        public readonly int Green;
        public readonly int Red;
        public readonly int Blue;
        public readonly int Brightness;

        private const double POTENTIAL_TOTAL = 255 * 4;
        private const int MAX_MATCH_VARIANCE = 20;

        public HistogramIdentifier(Bitmap image) {
            long r=0, g=0, b=0, brightness =0;
            for(int y = 0; y < image.Height; y++) {
                for(int x =0; x < image.Width; x++) {
                    Color color = image.GetPixel(x, y);
                    r += Convert.ToInt16(color.R);
                    g += Convert.ToInt16(color.G);
                    b += Convert.ToInt16(color.B);
                    
                }
            }
            long pixel_count = image.Height * image.Width;
            Red = (int)(r / pixel_count);
            Green = (int)(g / pixel_count);
            Blue = (int)(b / pixel_count);
            Brightness = (Red + Blue + Green) / 3;
        }

        public int Compare(HistogramIdentifier other) {
            int r = Math.Abs(Red - other.Red);
            int g = Math.Abs(Green- other.Green);
            int b = Math.Abs(Blue - other.Blue);
            int brightness = Math.Abs(Brightness - other.Brightness);
            double total = r + g + b + brightness;
            if(total > MAX_MATCH_VARIANCE) {
                return 0;
            }
           
            double percent = total / MAX_MATCH_VARIANCE;

            return (int)Math.Round((1.0-percent) * Comparitor.MAX_COMPARISON_RESULT);
        }
    }
}
