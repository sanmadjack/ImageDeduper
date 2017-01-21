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

        private SelectionCriteriaMode _Mode = SelectionCriteriaMode.ELSE;
        public SelectionCriteriaMode Mode { get {
                return _Mode;
            }
            set {
                _Mode = value;
                Properties.Settings.Default.Save();
            }
        }

        public ASelectionCriteria() {
        }

        public void PerformSelection(List<ComparableImage> images) {
            if (this.Mode == SelectionCriteriaMode.ELSE)
            {
                foreach (ComparableImage image in images)
                {
                    if (image.Selected) // Don't screw with existing selections
                        return;
                }
            }

            List<int> selectionIndexes = PerformSelectionInternal(images);

            switch (this.Mode)
            {
                case SelectionCriteriaMode.Disabled:
                    return;
                case SelectionCriteriaMode.ELSE:
                    foreach(int i in selectionIndexes)
                    {
                        images[i].Selected = true;
                    }
                    break;
                case SelectionCriteriaMode.AND:
                    foreach(ComparableImage img in images)
                    {
                        img.Selected = img.Selected && selectionIndexes.Contains(images.IndexOf(img));
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected abstract List<int> PerformSelectionInternal(List<ComparableImage> images);
    }
}
