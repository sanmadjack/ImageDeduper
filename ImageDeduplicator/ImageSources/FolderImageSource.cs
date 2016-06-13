using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ImageDeduplicator.ImageSources {
    public class FolderImageSource : AImageSource {
        private string path;

        public FolderImageSource(String path): base(path) {
            this.path = path;
        }

        protected override List<String> getImagesInternal() {
            return getImagesInPath(this.path);
        }

        private List<String> getImagesInPath(String path) {
            List<String> output = new List<String>();

            foreach (string f in Directory.GetFiles(path)) {
                output.Add(f);
            }
            bool recursive = true;
            if (recursive) {
                foreach (string d in Directory.GetDirectories(path)) {
                    output.AddRange(getImagesInPath(d));
                }
            }
            return output;
        }

    }
}
