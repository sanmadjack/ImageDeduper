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
        public long ImagePixelCount { get; private set; }

        public string ImageDimensions { get {
                return String.Concat(ImageHeight, "x", ImageWidth);
            }
        }
        public long ImageFileSize { get; private set; }

        public bool _Selected = false;
        public bool Selected {
            get { return _Selected; }
            set { _Selected = value; NotifyPropertyChanged("Selected"); }
        }

        public string SourceFolder { get; private set;  }

        public DuplicateImageSet CurrentDuplicateSet = null;

        public bool ImageLoaded { get; private set; }

        private BitmapImage _thumbImage;
        public BitmapImage Thumbnail {
            get {
                if (_thumbImage == null) {
                    RegenerateThumbnail();
                }
                return _thumbImage;
            }
        }
        public BitmapImage FullBitmapImage {
            get {
                return  new BitmapImage(new Uri(ImageFile));
            }
        }

        List<ComparisonResult> SimilarImages = new List<ComparisonResult>();

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
            Image image = null;
            using (MemoryStream ms = new MemoryStream()) {
                using (FileStream fs = File.OpenRead(ImageFile)) {
                    fs.CopyTo(ms);
                }
                ImageFileSize = ms.Length;

                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms)) {
                    FileHash = new FileHashIdentifier(br.ReadBytes((int)ms.Length));

                    ms.Seek(0, SeekOrigin.Begin);
                        image = Bitmap.FromStream(ms);
                    ImageHeight = image.Height;
                    ImageWidth = image.Width;
                    ImagePixelCount = ImageHeight * ImageWidth;
                }
                try {
                    Histogram = new HistogramIdentifier((Bitmap)image);
                } finally {
                    image.Dispose();
                }

                ImageLoaded = true;
            }
        }

        public void RegenerateThumbnail() {
            Image image = Bitmap.FromFile(ImageFile);
            UpdateThumbnail(image);
        }

        private int GeneratedThumbnailSize = -1;
        private void UpdateThumbnail(Image image) {
            int height = Properties.Settings.Default.ThumbnailHeight;

            if (height == GeneratedThumbnailSize)
                return;

            double old_height = image.Height;
            double old_width = image.Width;
            double ratio = height / old_height;
            int new_width = (int)Math.Round(ratio * image.Width);

            using (Image thumbNail = new Bitmap(new_width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb)) {
                using (Graphics g = Graphics.FromImage(thumbNail)) {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    Rectangle rect = new Rectangle(0, 0, new_width, height);
                    g.DrawImage(image, rect);
                }
                using (MemoryStream ms = new MemoryStream()) {
                    thumbNail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    this._thumbImage = null;
                    GeneratedThumbnailSize = height;

                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze();
                    _thumbImage = bi;
                    NotifyPropertyChanged("Thumbnail");
                }
            }

        }

        public double CompareImage(ComparableImage ci) {
            if (ci.ImageFile.Contains("379955")&& this.ImageFile.Contains("379953"))
                Console.Out.WriteLine("test");

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
