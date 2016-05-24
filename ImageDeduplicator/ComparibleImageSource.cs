using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator {
    public class ComparibleImageSource: INotifyPropertyChanged {
        private String _Name;
        public String Name { get { return _Name; } set { _Name = value; NotifyPropertyChanged("Name"); } }
        private bool _Locked = false;
        public bool Locked {
            get { return _Locked; }
            set { _Locked = value; NotifyPropertyChanged("Locked"); }
        }

        public ComparibleImageSource(String name) {
            this.Name = name;
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
