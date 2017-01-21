using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDeduplicator.SelectionCriteria {
    public abstract class AComparingSelectionCriteria: ASelectionCriteria {
        protected override List<int> PerformSelectionInternal(List<ComparableImage> images) {
            List<int> output = new List<int>();
            for (int i = 0; i < images.Count; i++) {
                for (int j = 0; j < images.Count; j++) {
                    if (j == i)
                        continue;

                    ComparableImage inferior = WhichIsInferior(images[i], images[j]);
                    if (inferior != null) {
                        output.Add(images.IndexOf(inferior));
                    }
                }
            }
            return output;
        }

        protected abstract ComparableImage WhichIsInferior(ComparableImage one, ComparableImage two);
    }
}
