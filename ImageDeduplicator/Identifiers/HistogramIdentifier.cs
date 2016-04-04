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
        public readonly double Green;
        public readonly double Red;
        public readonly double Blue;
        //public readonly double Brightness;

        private const double POTENTIAL_TOTAL = 255 * 3;
        private const int MAX_MATCH_VARIANCE = 30;

        public HistogramIdentifier(Bitmap image) {
            double r =0, g=0, b=0, brightness =0;
            for(int y = 0; y < image.Height; y++) {
                for(int x =0; x < image.Width; x++) {
                    Color color = image.GetPixel(x, y);
                    r += (int)color.R;
                    g += (int)color.G;
                    b += (int)color.B;
                    
                }
            }
            long pixel_count = image.Height * image.Width;
            Red = (r / pixel_count);
            Green = (g / pixel_count);
            Blue = (b / pixel_count);
            //Brightness = (Red + Blue + Green) / 3;
        }

        public double Compare(HistogramIdentifier other) {
            double r = Math.Abs(Red - other.Red);
            double g = Math.Abs(Green- other.Green);
            double b = Math.Abs(Blue - other.Blue);
            //double brightness = Math.Abs(Brightness - other.Brightness);
            double total = r + g + b ;
            if(total > MAX_MATCH_VARIANCE) {
                return 0;
            }
           
            double percent = total / MAX_MATCH_VARIANCE;

            return (1.0-percent) * Comparitor.MAX_COMPARISON_RESULT;
        }
    }
}
