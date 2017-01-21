using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.SelectionCriteria {
    public abstract class AEvaluatingSelectionCriteria : ASelectionCriteria {
        protected override List<int> PerformSelectionInternal(List<ComparableImage> images) {
            List<int> output = new List<int>();
            foreach (ComparableImage image in images) {
                bool result = DetermineIfSelectable(image);
                if (result)
                    output.Add(images.IndexOf(image));
            }
            return output;
        }

        protected abstract bool DetermineIfSelectable(ComparableImage image);
    }
}
