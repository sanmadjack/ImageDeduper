using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageDeduplicator {
    public class DuplicateImageSet: ObservableCollection<ComparableImage>, INotifyPropertyChanged {
        public BitmapImage Thumbnail {
            get {
                return this.First<ComparableImage>().Thumbnail;
            }
        }

        public DuplicateImageSet(ComparableImage starter_image) {
            starter_image.ComparisonResult = Comparitor.MAX_COMPARISON_RESULT;
            starter_image.CurrentDuplicateSet = this;
            this.Add(starter_image);
        }

        public void AddImage(ComparableImage image) {
            int i = 0;
            for(i = 0; i<this.Count; i++) {
                if(this[i].ComparisonResult<image.ComparisonResult) {
                    this.Insert(i, image);
                    return;
                }
            }
            image.CurrentDuplicateSet = this;

            App.Current.Dispatcher.Invoke((Action)(() => {
                this.Add(image);
            }));
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
