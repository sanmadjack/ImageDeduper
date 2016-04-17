using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageDeduplicator.Identifiers {
    class ScaledDifferenceIdentifer : AIdentifier {
        public const double SCALED_IMAGE_SIZE = 16;
        private const int MAX_MATCH_VARIANCE = 10;
        private const long MAX_TOTAL_DEVIANCE = MAX_MATCH_VARIANCE * (long)SCALED_IMAGE_SIZE * (long)SCALED_IMAGE_SIZE * 4;

        private Bitmap scaled_image;


        public ScaledDifferenceIdentifer(Bitmap image) {
            Bitmap thumbNail = new Bitmap((int)SCALED_IMAGE_SIZE, (int)SCALED_IMAGE_SIZE, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(thumbNail)) {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Rectangle rect = new Rectangle(0, 0, (int)SCALED_IMAGE_SIZE, (int)SCALED_IMAGE_SIZE);
                g.DrawImage(image, rect);
            }
            scaled_image = thumbNail;
        }

        public double Compare(ScaledDifferenceIdentifer other) {
            

            long r = 0, g = 0, b = 0, a= 0;
            lock(scaled_image) {
                for (int y = 0; y < scaled_image.Height; y++) {
                    for (int x = 0; x < scaled_image.Width; x++) {
                        System.Drawing.Color color = scaled_image.GetPixel(x, y);
                        System.Drawing.Color other_color = other.GetPixel(x, y);

                        r += Math.Abs(color.R - other_color.R);
                        g += Math.Abs(color.G - other_color.G);
                        b += Math.Abs(color.B - other_color.B);
                        a += Math.Abs(color.A - other_color.A);

                        if(r+g+b+a > MAX_TOTAL_DEVIANCE) {
                            // This is a shortcut so that drastically different images will quit early. 
                            return 0;
                        }
                    }
                }
            }
            double average_r, average_g, average_b, average_a;

            average_r = r / (SCALED_IMAGE_SIZE * SCALED_IMAGE_SIZE);
            average_g = g / (SCALED_IMAGE_SIZE * SCALED_IMAGE_SIZE);
            average_b = b / (SCALED_IMAGE_SIZE * SCALED_IMAGE_SIZE);
            average_a = a / (SCALED_IMAGE_SIZE * SCALED_IMAGE_SIZE);

            double average = (average_b + average_g + average_r + average_a) / 4;

            if (average > MAX_MATCH_VARIANCE) {
                return 0;
            }
           
            double percent = average / MAX_MATCH_VARIANCE;

            return (1.0-percent) * Comparitor.MAX_COMPARISON_RESULT;
        }

        public System.Drawing.Color GetPixel(int x, int y) {
            lock(scaled_image) {
                return scaled_image.GetPixel(x, y);
            }
        }
    }
}
