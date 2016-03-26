using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace ImageDeduplicator {
    public class Comparitor: ObservableCollection<DuplicateImageSet> {
        public int Fuzziness { get; set; }

        private Dictionary<FileInfo, ComparableImage> images = new Dictionary<FileInfo, ComparableImage>();

        public Comparitor() {
            Fuzziness = 100;
        }

        public async void LoadDirectory(DirectoryInfo dir, bool recursive)  {
            await Task.Run(() => {

            foreach(FileInfo f in dir.GetFiles()) {
                LoadImage(f);
            }
            if(recursive) {
                foreach(DirectoryInfo d in dir.GetDirectories()) {
                    LoadDirectory(d, true);
                }
            }
            });
        }

        public void LoadImage(FileInfo file) {
            ComparableImage ci = new ComparableImage(file);
            images.Add(file, ci);
            DuplicateImageSet ds = new DuplicateImageSet();
            ds.Add(ci);
            App.Current.Dispatcher.Invoke((Action)(() => {
                this.Add(ds);
            }));
        }


    }
}
