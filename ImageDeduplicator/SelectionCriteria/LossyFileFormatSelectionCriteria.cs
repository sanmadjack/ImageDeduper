using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Drawing.Imaging;

namespace ImageDeduplicator.SelectionCriteria {
    [Serializable]
    [System.Xml.Serialization.XmlRoot("file_name_regex")]
    public class LossyFileFormatSelectionCriteria : AEvaluatingSelectionCriteria {
        [XmlIgnore]
        public override string Name {
            get {
                return "Lossy File Format";
            }

            set {
                base.Name = value;
            }
        }

        public LossyFileFormatSelectionCriteria() { }

        protected override bool DetermineIfSelectable(ComparableImage image) {
            if(image.ImageFormat==ImageFormat.Jpeg) {
                return true;
            }
            return false;
        }
    }
}
