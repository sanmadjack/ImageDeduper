using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.SelectionCriteria {
    [System.Xml.Serialization.XmlRoot("selection_criteria")]
    [System.Xml.Serialization.XmlInclude(typeof(FileNameRegexSelectionCriteria))]
    [System.Xml.Serialization.XmlInclude(typeof(PathRegexSelectionCriteria))]
    [System.Xml.Serialization.XmlInclude(typeof(ParentDirectorySelectionCriteria))]
    [System.Xml.Serialization.XmlInclude(typeof(SmallerFileSizeSelectionCriteria))]
    [System.Xml.Serialization.XmlInclude(typeof(SmallerPixelCountSelectionCriteria))]
    public class SelectionCriteria: ObservableCollection<ASelectionCriteria>  {
        public void PerformSelection(List<ComparableImage> images) {
            foreach (ComparableImage image in images) {
                if (image.Selected) // Don't screw with existing selections
                    return;
            }
            foreach(ASelectionCriteria sc in this) {
                if(sc.Mode > SelectionCriteriaMode.Disabled)
                    sc.PerformSelection(images);
            }

            int selected = 0;
            foreach (ComparableImage image in images) {
                if (image.Selected)
                    selected++;
            }
            if (selected == images.Count) { // Need to better handle everything being selected, for now we just de-select everything
                foreach (ComparableImage image in images) {
                    image.Selected = false; 
                }
            }

        }

    }
}
