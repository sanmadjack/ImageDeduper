using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ImageDeduplicator.SelectionCriteria {
    [Serializable]
    [System.Xml.Serialization.XmlRoot("path_regex")]
    public class PathRegexSelectionCriteria : AEvaluatingSelectionCriteria {
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

        public override string Name {
            get {
                return "Path Regex: " + RegexString;
            }

            set {
                base.Name = value;
            }
        }

        public PathRegexSelectionCriteria() { }
        public PathRegexSelectionCriteria(string regex) {
            this.regex = new Regex(regex, RegexOptions.IgnoreCase);
            
        }

        protected override bool DetermineIfSelectable(ComparableImage image) {
            return regex.IsMatch(image.ImageFile);
        }
    }
}
