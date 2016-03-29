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

        public override string Name {
            get {
                return "File Name Regex: " + RegexString;
            }

            set {
                base.Name = value;
            }
        }
        public FileNameRegexSelectionCriteria() { }
        public FileNameRegexSelectionCriteria(string regex) {
            this.regex = new Regex(regex, RegexOptions.IgnoreCase);
            
        }

        protected override bool DetermineIfSelectable(ComparableImage image) {
            return regex.IsMatch(image.ImageFile);
        }
    }
}
