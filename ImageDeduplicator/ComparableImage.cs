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
        public string ImageFile { get; private set; }
        public string ImageFileName { get; private set; }
        public int ImageHeight { get; private set; }
        public int ImageWidth { get; private set; }
        public string ImageDimensions { get {
                return String.Concat(ImageHeight, "x", ImageWidth);
            }
        }
        public long ImageSize { get; private set; }

        public bool _Selected = false;
        public bool Selected {
            get { return _Selected; }
            set { _Selected = value; NotifyPropertyChanged("Selected"); }
        }

        public string SourceFolder { get; private set;  }

        public int ComparisonResult { get; set; }
        public DuplicateImageSet CurrentDuplicateSet = null;

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
        public BitmapImage FullBitmapImage {
            get {
                return  new BitmapImage(new Uri(ImageFile));
            }
        }

        FileHashIdentifier FileHash;
        ImageHashIdentifier ImageHash;
        HistogramIdentifier Histogram;

        public ComparableImage(string source_folder, string image_file) {
            this.ImageFile = image_file;
            this.SourceFolder = source_folder;
        }

        public void LoadImage() {
            if (!File.Exists(ImageFile))
                throw new FileNotFoundException("Could not find specified file", ImageFile);

            ImageFileName = Path.GetFileName(ImageFile);
            Image image;
            using (MemoryStream ms = new MemoryStream()) {
                using (FileStream fs = File.OpenRead(ImageFile)) {
                    fs.CopyTo(ms);
                }
                ImageSize = ms.Length;

                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms)) {
                    FileHash = new FileHashIdentifier(br.ReadBytes((int)ms.Length));

                    ms.Seek(0, SeekOrigin.Begin);

                    image = Bitmap.FromStream(ms);
                    ImageHeight = image.Height;
                    ImageWidth = image.Width;
                }
                Histogram = new HistogramIdentifier((Bitmap)image);

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

            using (Image thumbNail = new Bitmap(new_width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb)) {
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

        public int CompareImage(ComparableImage ci) {
            if (ci.FileHash.IsMatch(this.FileHash))
                return Comparitor.MAX_COMPARISON_RESULT;

            return ci.Histogram.Compare(this.Histogram);

            //return 0;
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
