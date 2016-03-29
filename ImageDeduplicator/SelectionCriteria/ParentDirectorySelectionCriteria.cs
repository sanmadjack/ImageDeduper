using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
namespace ImageDeduplicator.SelectionCriteria {
    [Serializable]
    [System.Xml.Serialization.XmlRoot("parent_directory")]
    public class ParentDirectorySelectionCriteria : AEvaluatingSelectionCriteria {
        [XmlElement("path")]
        public string Path{
            get {
                return dir.ToString();
            }
            set {
                dir = new DirectoryInfo(value);
            }
        }
        private DirectoryInfo dir;

        public override string Name {
            get {
                return "Parent Directory: " + Path;
            }

            set {
                base.Name = value;
            }
        }
        public ParentDirectorySelectionCriteria() { }
        public ParentDirectorySelectionCriteria(string path) {
            dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new Exception("Specified directory does not exist");
        }

        protected override bool DetermineIfSelectable(ComparableImage image) {
            if (image.ImageFile.StartsWith(dir.FullName)) {
                return true;
            }
            return false;
        }
    }
}
