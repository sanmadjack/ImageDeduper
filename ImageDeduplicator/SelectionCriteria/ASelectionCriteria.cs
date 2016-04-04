using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImageDeduplicator.SelectionCriteria {
    public abstract class ASelectionCriteria {

        [XmlIgnore]
        public virtual String Name { get; set; }

        public void PerformSelection(List<ComparableImage> images) {
            foreach (ComparableImage image in images) {
                if (image.Selected) // Don't screw with existing selections
                    return;
            }

            PerformSelectionInternal(images);
        }

        protected abstract void PerformSelectionInternal(List<ComparableImage> images);
    }
}
