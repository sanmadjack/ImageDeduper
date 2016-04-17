using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator {
    public class ComparisonSet: ObservableCollection<ComparisonResult>, INotifyPropertyChanged {

        private static bool _ScaleImage = false;
        public bool ScaleImage {
            get {
                return _ScaleImage;
            }
            set {
                _ScaleImage = value;
                NotifyPropertyChanged("ScaleImage");
            }
        }

        private bool HasTempImage = false;

        public void AddImage(ComparisonResult cr) {
            if (HasTempImage) {
                if(this.Count>0)
                    this.RemoveAt(0);
                HasTempImage = false;
            }

            int i = this.IndexOf(cr);
            if (i!=-1) {
                return;
            }

            this.Insert(0, cr);
            HasTempImage = true;
        }


        public void ToggleImage(ComparisonResult cr) {
            int i = this.IndexOf(cr);
            if(i==-1) {
                return;
            }
            if (i == 0 && HasTempImage) {
                HasTempImage = false;
            } else {
                if (HasTempImage) {
                    this.RemoveAt(0);
                }
                this.Remove(cr);
                this.Insert(0, cr);
                HasTempImage = true;
            }

            cr.Comparing = true;

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
