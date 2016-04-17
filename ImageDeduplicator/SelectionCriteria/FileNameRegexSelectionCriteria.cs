using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ImageDeduplicator.SelectionCriteria {
    [Serializable]
    [System.Xml.Serialization.XmlRoot("file_name_regex")]
    public class FileNameRegexSelectionCriteria : AEvaluatingSelectionCriteria {
        [XmlElement("regex")]
        public string RegexString {
            get {
                return regex.ToString();
            }
            set {
                regex = new Regex(value);
            }
        }
        private Regex regex;

        [XmlElement("invert")]
        public bool Invert {
            get; set;
        }

        [XmlIgnore]
        public override string Name {
            get {
                StringBuilder output = new StringBuilder();
                output.Append("File Name Regex: ");
                output.Append(RegexString);
                if (Invert)
                    output.Append(" (Invert)");
                return output.ToString();
            }

            set {
                base.Name = value;
            }
        }

        public FileNameRegexSelectionCriteria() { }
        public FileNameRegexSelectionCriteria(string regex, bool invert) {
            this.regex = new Regex(regex, RegexOptions.IgnoreCase);
            this.Invert = invert;
        }

        protected override bool DetermineIfSelectable(ComparableImage image) {
            if (Invert) {
                return !regex.IsMatch(image.ImageFileName);
            } else {
                return regex.IsMatch(image.ImageFileName);
            }
        }
    }
}
