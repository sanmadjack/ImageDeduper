using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.SelectionCriteria {
    public abstract class AEvaluatingSelectionCriteria : ASelectionCriteria {
        protected override void PerformSelectionInternal(List<ComparableImage> images) {
            foreach(ComparableImage image in images) {
                bool result = DetermineIfSelectable(image);
                if (result && !image.Selected)
                    image.Selected = true;
            }
        }

        protected abstract bool DetermineIfSelectable(ComparableImage image);
    }
}
