using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.SelectionCriteria {
    [Serializable]
    [System.Xml.Serialization.XmlRoot("smaller_file_size")]
    public class SmallerFileSizeSelectionCriteria : AComparingSelectionCriteria {

        public SmallerFileSizeSelectionCriteria() {
            Name = "Smaller File Size";
        }

        protected override ComparableImage WhichIsInferior(ComparableImage one, ComparableImage two) {

            if (one.ImageFileSize > two.ImageFileSize) {
                return two;
            } else if (one.ImageFileSize < two.ImageFileSize) {
                return one;
            }
            return null;
        }
    }
}
