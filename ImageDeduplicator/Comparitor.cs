using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ImageDeduplicator {
    public class Comparitor : ObservableCollection<DuplicateImageSet>, INotifyPropertyChanged {
        public double Fuzziness { get; set; }

        public const int MAX_COMPARISON_RESULT = 100;

        public int LoadProgress {
            get {
                double total_to_load = ImagesToLoad.Count + ImagesToCompare.Count + images.Count;
                double total_loaded = images.Count;
                double percent = total_loaded / total_to_load;
                return (int)Math.Round(percent * 100);
            }
        }

        private List<ComparableImage> images = new List<ComparableImage>();

        private BackgroundWorker imageCompareWorkers = new BackgroundWorker();
        private List<BackgroundWorker> imageLoadWorkers = new List<BackgroundWorker>();

        private Queue<ComparableImage> ImagesToLoad = new Queue<ComparableImage>();
        private Queue<ComparableImage> ImagesToCompare = new Queue<ComparableImage>();


        private bool StopAllThreads = false;

        public Comparitor() {
            Fuzziness = 99;

            imageCompareWorkers.DoWork += ImageCompareWorkers_DoWork;
            for (int i = 0; i < Environment.ProcessorCount; i++) {
                BackgroundWorker imageLoadWorker = new BackgroundWorker();
                imageLoadWorker.DoWork += ImageLoadWorker_DoWork;
                imageLoadWorker.RunWorkerCompleted += ImageLoadWorker_RunWorkerCompleted;
                imageLoadWorkers.Add(imageLoadWorker);
            }
        }

        private void ImageCompareWorkers_DoWork(object sender, DoWorkEventArgs e) {
            ComparableImage ci = null;
            lock (ImagesToCompare) {
                if (ImagesToCompare.Count == 0)
                    return;

                ci = ImagesToCompare.Dequeue();
            }
            while (ci != null) {

                FindDupes(ci);
                lock(images) {
                    images.Add(ci);
                }

                NotifyPropertyChanged("LoadProgress");

                lock (ImagesToCompare) {
                    if (ImagesToCompare.Count == 0)
                        return;
                    ci = ImagesToCompare.Dequeue();
                }
            }

        }

        private void ImageLoadWorker_DoWork(object sender, DoWorkEventArgs e) {
            ComparableImage ci;
            lock (ImagesToLoad) {
                if (ImagesToLoad.Count > 0) {
                    ci = ImagesToLoad.Dequeue();
                } else {
                    return;
                }
            }
            while (ci != null) {

                try {
                    ci.LoadImage();
                    lock (ImagesToCompare) {
                        ImagesToCompare.Enqueue(ci);
                    }

                    if (!imageCompareWorkers.IsBusy) {
                        imageCompareWorkers.RunWorkerAsync();
                    }
                } catch (FileNotFoundException ex) {
                    // Files gone, no need to pay it any attention
                }
                lock (ImagesToLoad) {
                    if (ImagesToLoad.Count > 0) {
                        ci = ImagesToLoad.Dequeue();
                    } else {
                        return;
                    }
                }
            }
        }

        private void ImageLoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //throw new NotImplementedException();
        }

        public async void LoadDirectoryAsync(string dir, bool recursive) {
            await Task.Run(() => {
                List<ComparableImage> images = LoadDirectoryInternal(dir, recursive);

                lock (ImagesToLoad) {
                    foreach (ComparableImage ci in images) {
                        ImagesToLoad.Enqueue(ci);
                    }
                }

                NotifyPropertyChanged("LoadProgress");
                foreach (BackgroundWorker worker in this.imageLoadWorkers) {
                    worker.RunWorkerAsync();
                }
            });
        }

        private List<ComparableImage> LoadDirectoryInternal(string dir, bool recursive) {
            List<ComparableImage> images = new List<ComparableImage>();

            foreach (string f in Directory.GetFiles(dir)) {
                images.Add(new ComparableImage(dir, f));
            }
            if (recursive) {
                foreach (string d in Directory.GetDirectories(dir)) {
                    images.AddRange(LoadDirectoryInternal(d, true));
                }
            }
            return images;
        }

        private void FindDupes(ComparableImage ci) {
            ComparableImage candidate_image = null;
            int candidate_result = 0;

            foreach (ComparableImage other_image in this.images) {
                if (ci.ImageFile == other_image.ImageFile) // Same image checl
                    continue;

                int result = other_image.CompareImage(ci);
                if (result > candidate_result) {
                    candidate_result = result;
                    candidate_image = other_image;
                }
            }

            if (candidate_image != null && candidate_result >= Fuzziness) {
                DuplicateImageSet ds;
                if (candidate_image.CurrentDuplicateSet == null) {
                    ds = new DuplicateImageSet(candidate_image);
                } else {
                    // This will get more complicated
                    DuplicateImageSet currentSet = candidate_image.CurrentDuplicateSet;
                    ComparableImage first_image = currentSet.First<ComparableImage>();
                    if (first_image != candidate_image) {
                        int first_image_result = first_image.CompareImage(ci);
                        if (first_image_result < Fuzziness) {
                            //This means that the current image doesn't match the dupe set's starter image enough to qualify
                            // So we move the image it matches out of that group and into the current image's group
                            App.Current.Dispatcher.Invoke((Action)(() => {
                                currentSet.Remove(candidate_image);
                                if (currentSet.Count == 1) {
                                    lock (this) {
                                        if (this.Contains(currentSet))
                                            this.Remove(currentSet);
                                    }
                                }
                            }));
                            ds = new DuplicateImageSet(candidate_image);

                        } else {
                            ds = currentSet;
                        }
                    } else {
                        ds = currentSet;
                    }

                }

                ds.AddImage(ci);

                App.Current.Dispatcher.Invoke((Action)(() => {
                    lock (this) {
                        if (!this.Contains(ds))
                            this.Add(ds);
                    }
                }));
            }


        }

        public void RemoveImage(ComparableImage ci) {
            DuplicateImageSet dis = ci.CurrentDuplicateSet;
            if(dis!= null) {
                lock(dis) {
                    dis.Remove(ci);
                    lock(this) {
                        if (this.Contains(dis)) {
                            this.Remove(dis);
                        }
                    }
                }
            }
            lock (images) {
                if (this.images.Contains(ci))
                    this.images.Remove(ci);
            }
        }

        #region INotify Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
