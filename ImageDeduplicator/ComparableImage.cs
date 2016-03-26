using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using ImageDeduplicator.Identifiers;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageDeduplicator {
    public class ComparableImage : INotifyPropertyChanged {
        public readonly FileInfo ImageFile;

        public bool ImageLoaded { get; private set; }
        private MemoryStream _thumbStream;
        private BitmapImage _thumbImage;
        public BitmapImage Thumbnail {
            get {
                if (_thumbImage == null && ImageLoaded) {
                    this._thumbStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = _thumbStream;
                    bi.EndInit();
                    _thumbImage = bi;
                }
                return _thumbImage;
            }
        }

        FileHashIdentifier FIleHash;
        ImageHashIdentifier ImageHash;



        public ComparableImage(System.IO.FileInfo image_file) {
            ImageFile = image_file;
            LoadImage(image_file);
        }

        private void LoadImage(System.IO.FileInfo image_file) {
            Image image;
            using (MemoryStream ms = new MemoryStream()) {
                using (FileStream fs = image_file.OpenRead()) {
                    fs.CopyTo(ms);
                }
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms)) {
                    FIleHash = new FileHashIdentifier(br.ReadBytes((int)ms.Length));

                    ms.Seek(0, SeekOrigin.Begin);

                    image = Bitmap.FromStream(ms);
                }
            }
            try {
                // ImageHash = new ImageHashIdentifier(image);
                UpdateThumbnail(image, 100);

            } finally {
                image.Dispose();
            }
        }

        private void UpdateThumbnail(Image image, int height) {
            double old_height = image.Height;
            double old_width = image.Width;
            double ratio = height / old_height;
            int new_width = (int)Math.Round(ratio * image.Width);

            using (Image thumbNail = new Bitmap(new_width, height, image.PixelFormat)) {
                using (Graphics g = Graphics.FromImage(thumbNail)) {
                    g.CompositingQuality = CompositingQuality.Default;
                    g.SmoothingMode = SmoothingMode.Default;
                    g.InterpolationMode = InterpolationMode.Default;
                    Rectangle rect = new Rectangle(0, 0, new_width, height);
                    g.DrawImage(image, rect);
                }

                if (this._thumbStream != null)
                    this._thumbStream.Dispose();

                this._thumbStream = new MemoryStream();

                thumbNail.Save(this._thumbStream, System.Drawing.Imaging.ImageFormat.Png);

                this._thumbImage = null;
                ImageLoaded = true;

            }

        }


        #region INotify Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
