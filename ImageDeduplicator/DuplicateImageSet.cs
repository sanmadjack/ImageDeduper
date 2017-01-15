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
    public class DuplicateImageSet: ObservableCollection<ComparisonResult>, INotifyPropertyChanged {
        public BitmapImage Thumbnail {
            get {
                return this.First<ComparisonResult>().Image.Thumbnail;
            }
        }

        public DuplicateImageSet(ComparableImage starter_image) {
            starter_image.CurrentDuplicateSet = this;
            this.Add(new ComparisonResult {
                Image = starter_image,
                Result = Comparitor.MAX_COMPARISON_RESULT
            });
        }

        public bool ContainsImage(ComparableImage ci) {
            
            foreach (ComparisonResult cr in this) {
                if (cr.Image == ci) {
                    return true;
                }
            }
            return false;
        }

        public List<ComparableImage> GetUnselectedImages() {
            List<ComparableImage> output = new List<ComparableImage>();
            foreach(ComparisonResult result in this) {
                if(!result.Image.Selected) {
                    output.Add(result.Image);
                }
            }
            return output;
        }

        public  virtual void ReplaceImage(ComparableImage original, ComparableImage newImage) {
            ComparisonResult cr = this.GetImageResult(original);
            cr.Image = newImage;
          //  this.RemoveImage(original);
        //    this.AddImage(newImage);
        }

        public List<ComparableImage> GetImages() {
            List<ComparableImage> output = new List<ComparableImage>();
            foreach(ComparisonResult cr in this) {
                output.Add(cr.Image);
            }
            return output;
        }

        public int RemoveImage(ComparableImage ci) {
            ComparisonResult cr = GetImageResult(ci);
            int i = this.IndexOf(cr);
            this.Remove(cr);
            return i;
        }

        public ComparisonResult GetImageResult(ComparableImage ci) {
            foreach (ComparisonResult cr in this) {
                if (cr.Image == ci) {
                    return cr;
                }
            }
            return null;
        }

        public void AddImage(ComparableImage image) {
            image.CurrentDuplicateSet = this;

            ComparisonResult cr = this.First<ComparisonResult>().Image.GetComparisonResultForImage(image);
            int i = 0;
            for (i = 0; i<this.Count; i++) {
                if(this[i].Result < cr.Result) {
                    App.Current.Dispatcher.Invoke((Action)(() => {
                        this.Insert(i, cr);
                    }));
                    return;
                }
            }

            App.Current.Dispatcher.Invoke((Action)(() => {
                this.Add(cr);
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
