﻿using System;
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
using System.Security.Cryptography;
using ImageMagick;

namespace ImageDeduplicator {
    public class ComparableImage : INotifyPropertyChanged, IComparable {
        public string DisplayName { get {
                if (!string.IsNullOrEmpty(OverrideDisplayName))
                    return OverrideDisplayName;

                if (String.IsNullOrWhiteSpace(AlternativeName))
                    return ImageFileName;
                else
                    return ImageFileName + " - " + AlternativeName;
            }
        }

        public String OverrideDisplayName = String.Empty;


        private String AlternativeName = "";

        public string ImageFile { get; private set; }
        public string ImageProxyFile { get {
                if(String.IsNullOrWhiteSpace(ImageFile))
                {
                    return "";
                }
                var preExtension = Path.GetFileNameWithoutExtension(ImageFileName);
                return Path.Combine(ImageFilePath, preExtension + ".proxy.png");
            }
        }
        public string ImageFileForFullView
        { get
            {
                if (!String.IsNullOrWhiteSpace(ImageProxyFile) && File.Exists(ImageProxyFile))
                {
                    return ImageProxyFile;
                }
                return ImageFile;
            }
           
        }
        public string ImageFileExtension
        {
            get {
                if (String.IsNullOrWhiteSpace(ImageFile))
                {
                    return "";
                }
                return Path.GetExtension(ImageFile);
            }
        }
        public string ImageFilePath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(ImageFile))
                {
                    return "";
                }
                return Path.GetDirectoryName(ImageFile);
            }
        }
        public string ImageFileName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(ImageFile))
                {
                    return "";
                }
                return Path.GetFileName(ImageFile);
            }
        }
        public string ImageThumbnailFile { get; private set; }
        public int ImageHeight { get; private set; }
        public int ImageWidth { get; private set; }
        public long ImagePixelCount { get; private set; }

        private string _Checksum;

        public string  Checksum
        {
            get
            {
                if(String.IsNullOrWhiteSpace(_Checksum))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(this.ImageFile))
                        {
                            var hash = md5.ComputeHash(stream);
                            _Checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        }
                    }

                }
                return _Checksum;
            }
        }

        public string ImageDimensions { get {
                return String.Concat(ImageWidth, "x", ImageHeight);
            }
        }
        public long ImageFileSize { get; private set; }
        public string ImageFileSizeString { get {
                return String.Format("{0:n0}", ImageFileSize);
            }
        }

        public bool _Selected = false;
        public bool Selected {
            get { return _Selected; }
            set {
                if (this.Source.Locked) {
                    _Selected = false;
                } else {
                    _Selected = value;
                }
                NotifyPropertyChanged("Selected");
            }
        }

        public Object InternalIdentifier = null;


        public AImageSource Source { get; private set;  }

        public DuplicateImageSet CurrentDuplicateSet = null;

        public bool ImageLoaded { get; private set; }
        public System.Drawing.Imaging.ImageFormat ImageFormat { get; private set; }

        private BitmapImage _thumbImage;
        public BitmapImage Thumbnail {
            get {
                if (_thumbImage == null) {
                    RegenerateThumbnail();
                }
                return _thumbImage;
            }
        }


        List<ComparisonResult> SimilarImages = new List<ComparisonResult>();

        FileHashIdentifier FileHash;
        ImageHashIdentifier ImageHash;
        HistogramIdentifier Histogram;
        ScaledDifferenceIdentifer Scaled;

        public ComparableImage(AImageSource source, string image_file, string image_thumbnail = "", String alternativeName = "") {
            this.ImageFile = image_file;
            this.ImageThumbnailFile = image_thumbnail;
            this.Source = source;
            this.AlternativeName = alternativeName;
        }

        public bool ContainsResultFor(ComparableImage ci) {
            lock(this.SimilarImages) {
                foreach (ComparisonResult cr in this.SimilarImages) {
                    if (cr.Image == ci) {
                        return true;
                    }
                }
                return false;
            }
        }
        public void AddResult(ComparableImage ci, double result) {
            ComparisonResult cr = new ComparisonResult {
                Image = ci,
                Result = result
            };
            lock (this.SimilarImages) {
                this.SimilarImages.Add(cr);
                this.SimilarImages.Sort((x, y) => y.Result.CompareTo(x.Result));
            }
        }

        public  void RemoveResultFor(ComparableImage ci) {
            ComparisonResult cr = this.GetComparisonResultForImage(ci);
            lock (this.SimilarImages) {
                this.SimilarImages.Remove(cr);
            }
        }

        public ComparisonResult GetComparisonResultForImage(ComparableImage ci) {
            lock (this.SimilarImages) {
                foreach (ComparisonResult cr in this.SimilarImages) {
                    if (cr.Image == ci) {
                        return cr;
                    }
                }
            }
            return null;
        }

        public ComparisonResult GetTopSimilarImage() {
            if (this.SimilarImages.Count == 0)
                return null;
            lock (this.SimilarImages) {
                return SimilarImages.First<ComparisonResult>();
            }
        }

        public List<ComparisonResult> GetSimilarImages() {
            lock (this.SimilarImages) {
                return SimilarImages.ToList<ComparisonResult>();
            }
        }

        public void ClearSimilarImages() {
            lock (this.SimilarImages) {
                this.SimilarImages.Clear();
            }
        }

        public void LoadImage() {
            var fi = new FileInfo(ImageFile);
            if (!fi.Exists)
                throw new FileNotFoundException("Could not find specified file", ImageFile);

            Image image = null;
            var fileToLoad = ImageFile;
            if (ImageFileExtension==".heic" || ImageFileExtension == ".webp")
            {
                fileToLoad = ImageProxyFile;
                ImageThumbnailFile = ImageProxyFile;
                if (!File.Exists(ImageProxyFile))
                {
                    // Create ze proxy!
                    using (MagickImage mImage = new MagickImage(ImageFile))
                    {
                        // Save frame as jpg
                        mImage.Write(ImageProxyFile);
                    }

                }
            }
            ImageFileSize = fi.Length;

            using (MemoryStream ms = new MemoryStream()) {

                using (FileStream fs = File.OpenRead(fileToLoad)) {
                    fs.CopyTo(ms);
                }                

                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader br = new BinaryReader(ms)) {
                    FileHash = new FileHashIdentifier(br.ReadBytes((int)ms.Length));

                    ms.Seek(0, SeekOrigin.Begin);
                        image = Bitmap.FromStream(ms);
                    ImageHeight = image.Height;
                    ImageWidth = image.Width;
                    ImageFormat = image.RawFormat;
                    ImagePixelCount = ImageHeight * ImageWidth;
                }
                try {
                    //Histogram = new HistogramIdentifier((Bitmap)image);
                    Scaled = new ScaledDifferenceIdentifer((Bitmap)image);
                } finally {
                    image.Dispose();
                }

                ImageLoaded = true;
            }
        }

        public void RegenerateThumbnail() {
            Image image;
            if(String.IsNullOrWhiteSpace(this.ImageThumbnailFile) || !File.Exists(ImageThumbnailFile))
                image = Bitmap.FromFile(ImageFile);
            else
                image = Bitmap.FromFile(ImageThumbnailFile);

            UpdateThumbnail(image);
        }

        private int GeneratedThumbnailSize = -1;
        private void UpdateThumbnail(Image image) {
            try {

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
            }catch(Exception e) {
                Console.Out.WriteLine(e.Message);
            }

        }

        public double CompareImage(ComparableImage ci) {

            if (ci.FileHash.IsMatch(this.FileHash))
                return Comparitor.MAX_COMPARISON_RESULT;

            double result;// = ci.Histogram.Compare(this.Histogram);

            result = ci.Scaled.Compare(this.Scaled);

            return result;
        }

        public void Delete() {
            this.Source.deleteImage(this);
        }

        public ComparableImage Merge(ComparableImage target) {
            return this.Source.mergeImages(this, target);
        }

        #region INotify Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int CompareTo(object obj) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
