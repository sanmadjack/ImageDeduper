using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator {
    public class ComparisonSet: ObservableCollection<ComparableImage> {

        private ComparableImage temp_image = null;

        public void SetTempImage(ComparableImage image) {
            if(temp_image!=null) {
                this.Remove(temp_image);
            }
            if(!this.Contains(image)) {
                this.temp_image = image;
                this.Insert(0, temp_image);
            }
        }


        public void ToggleCurrentImageSave(ComparableImage image) {
            if (temp_image == null||temp_image!= image) {
                if(this.Contains(image)) {
                    this.Remove(image);
                }
                this.Insert(0, image);
                temp_image = image;
            } else if(temp_image==image) { 
                temp_image = null;
            }
        }
    }
}
