using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.SelectionCriteria {
    [Serializable]
    [System.Xml.Serialization.XmlRoot("smaller_pixel_count")]
    public class SmallerPixelCountSelectionCriteria : AComparingSelectionCriteria {

        public SmallerPixelCountSelectionCriteria() {
            Name = "Smaller Pixel Count";
        }

        protected override ComparableImage WhichIsInferior(ComparableImage one, ComparableImage two) {

            if (one.ImagePixelCount > two.ImagePixelCount) {
                return two;
            } else if (one.ImagePixelCount < two.ImagePixelCount) {
                return one;
            }
            return null;
        }
    }
}
