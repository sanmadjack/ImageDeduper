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

        public override ComparableImage mergeImages(ComparableImage source, ComparableImage target)
        {
            FileInfo f = new FileInfo(source.ImageFile);
            FileInfo fTarget = new FileInfo(target.ImageFile);
            String newTarget = Path.Combine(fTarget.DirectoryName, f.Name);
            this.deleteImageFromDisk(source);
            fTarget.MoveTo(newTarget);
            return new ComparableImage(this, newTarget);
        }

    }
}
