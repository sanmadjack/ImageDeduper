using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace ImageDeduplicator {
    public class ComparisonResult : INotifyPropertyChanged {
        public double ResultString {
            get { return Math.Round(Result, 2); }
        }
        public double Result {
            get; set;
        }
        public ComparableImage Image {
            get; set;
        }

        private static double _Zoom = 1;
        public static int ZoomLevel { get { return (int)Math.Round(_Zoom*100); }
            set {
                _Zoom = (double)value / 100;
            }
        }
        public int Zoom {
            get { return (int)Math.Round(_Zoom * 100); }
            set {
                _Zoom = (double)value / 100;
                NotifyPropertyChanged("Zoom");
                NotifyPropertyChanged("ImageHeight");
                NotifyPropertyChanged("ImageWidth");
            }
        }

        public double ImageHeight {
            get {
                return Image.ImageHeight * _Zoom;
            }
        }
        public double ImageWidth {
            get {
                return Image.ImageWidth * _Zoom;
            }
        }

        public BitmapImage FullBitmapImage {
            get {
                try {
                    using (Stream file = File.OpenRead(Image.ImageFileForFullView)) {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = file;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                } catch (System.IO.IOException ex) {
                    // Do something!
                    return null;
                }
            }
        }

        public bool _Comparing = false;
        public bool Comparing {
            get { return _Comparing; }
            set { _Comparing = value; NotifyPropertyChanged("Comparing"); }
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
