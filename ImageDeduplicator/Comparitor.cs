using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Threading;
using ImageDeduplicator.SelectionCriteria;
using System.Xml.Serialization;

namespace ImageDeduplicator {
    public class Comparitor : ObservableCollection<DuplicateImageSet>, INotifyPropertyChanged {
        public ObservableCollection<AImageSource> Sources = new ObservableCollection<AImageSource>();


        public double Fuzziness {
            get {
                return Properties.Settings.Default.Similarity;
            }
            set {
                Properties.Settings.Default.Similarity = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("Fuzziness");
            }
        }


        public const double MAX_COMPARISON_RESULT = 100;

        public String LoadProgressText {
            get {
                StringBuilder output = new StringBuilder();
                output.Append(images.Count);
                output.Append("/");
                output.Append(ImagesToLoad.Count + ImagesToCompare.Count + images.Count);
                if(this.Count>0) {
                    output.Append(" - ");
                    output.Append(this.Count.ToString());
                    output.Append(" Duplicate Groups Found");
                }
                return output.ToString();
            }
        }
        public double LoadProgressDouble {
            get {
                return ((double)LoadProgress) / 100;
            }
        }
        public int LoadProgress {
            get {
                double total_to_load = ImagesToLoad.Count + ImagesToCompare.Count + images.Count;
                double total_loaded = images.Count;
                double percent = total_loaded / total_to_load;
                return (int)Math.Round(percent * 100);
            }
        }

        private bool _OnlyDifferentSources = false;
        public bool OnlyDifferentSources { get { return _OnlyDifferentSources; } set { _OnlyDifferentSources = value; NotifyPropertyChanged("OnlyDifferentSources"); this.Reset(); } }

        private bool _OnlyDifferentFolders = false;
        public bool OnlyDifferentFolders { get { return _OnlyDifferentFolders; } set { _OnlyDifferentFolders = value; NotifyPropertyChanged("OnlyDifferentFolders"); this.Reset(); } }
        private bool _OnlyDifferentSizes= false;
        public bool OnlyDifferentSizes { get { return _OnlyDifferentSizes; } set { _OnlyDifferentSizes = value; NotifyPropertyChanged("OnlyDifferentSizes "); this.Reset(); } }


        private bool _OnlyChecksum = false;
        public bool OnlyChecksum { get { return _OnlyChecksum; } set { _OnlyChecksum = value; NotifyPropertyChanged("OnlyChecksum"); this.Reset(); } }

        public int ThumbnailHeight {
            get {
                return Properties.Settings.Default.ThumbnailHeight;
            }
            set {
                Properties.Settings.Default.ThumbnailHeight = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("ThumbnailSize");
            }
        }

        private List<ComparableImage> images = new List<ComparableImage>();

        private BackgroundWorker imageCompareWorkers = new BackgroundWorker();
        private List<BackgroundWorker> imageLoadWorkers = new List<BackgroundWorker>();

        private Queue<ComparableImage> ImagesToLoad = new Queue<ComparableImage>();
        private Queue<ComparableImage> ImagesToCompare = new Queue<ComparableImage>();


        public SelectionCriteria.SelectionCriteria selectors {
            get {
                if (Properties.Settings.Default.SelectionCriteria == null) {
                    SelectionCriteria.SelectionCriteria sc = new SelectionCriteria.SelectionCriteria();
                    sc.Add(new SmallerPixelCountSelectionCriteria());
                    Properties.Settings.Default.SelectionCriteria = sc;
                    Properties.Settings.Default.Save();
                }
                return Properties.Settings.Default.SelectionCriteria;
            }
        }


        private bool StopAllThreads = false;

        public Comparitor() {
            imageCompareWorkers.DoWork += ImageCompareWorkers_DoWork;
            for (int i = 0; i < Environment.ProcessorCount; i++) {
                BackgroundWorker imageLoadWorker = new BackgroundWorker();
                imageLoadWorker.DoWork += ImageLoadWorker_DoWork;
                imageLoadWorker.RunWorkerCompleted += ImageLoadWorker_RunWorkerCompleted;
                imageLoadWorkers.Add(imageLoadWorker);
            }

        }

        public void Reset() {
            lock (Sources)
                this.Sources.Clear();
            lock(ImagesToLoad) 
                this.ImagesToLoad.Clear();
            lock(ImagesToCompare)
                this.ImagesToCompare.Clear();
            lock(this.images)
                this.images.Clear();
            lock(this)
                this.Clear();
        }

        private void SendProgressUpdate() {
            NotifyPropertyChanged("LoadProgress");
            NotifyPropertyChanged("LoadProgressDouble");
            NotifyPropertyChanged("LoadProgressText");
        }

        private ComparableImage SafelyGetNextImage(ComparableImage image) {
            lock (images) {
                if(images.Count==0) {
                    return null;
                }
                if (image==null) {
                    return images[0];
                }
                int i = images.IndexOf(image);
                if(i ==-1) {
                    throw new Exception("Shit");
                }
                if((i+2)>images.Count) {
                    return null;
                }
                return images[i + 1];
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
                if (File.Exists(ci.ImageFile)) {
                    CalculateSimilarities(ci);
                    FindDupes(ci);
                    lock (images) {
                        images.Add(ci);
                    }
                }

                SendProgressUpdate();

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
                    lock (Sources) {
                        if (!Sources.Contains(ci.Source))
                            Sources.Add(ci.Source);
                    }

                    lock (ImagesToCompare) {
                        ImagesToCompare.Enqueue(ci);
                    }

                    if (!imageCompareWorkers.IsBusy) {
                        imageCompareWorkers.RunWorkerAsync();
                    }
                } catch (FileNotFoundException ex) {
                    // Files gone, no need to pay it any attention
                } catch (Exception ex) {
                    Console.Out.WriteLine(ex.Message);
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

        public async void LoadAsync(AImageSource source) {
            await Task.Run(() => {
                List<ComparableImage> images = source.getImages();

                lock (ImagesToLoad) {
                    foreach (ComparableImage ci in images) {
                        ImagesToLoad.Enqueue(ci);
                    }
                }

                SendProgressUpdate();
                foreach (BackgroundWorker worker in this.imageLoadWorkers) {
                    if(!worker.IsBusy)
                        worker.RunWorkerAsync();
                }
            });
        }

        public async void LoadFilesAsync(AImageSource source) {
            await Task.Run(() => {
                List<ComparableImage> images = source.getImages();

                lock (ImagesToLoad) {
                    foreach (ComparableImage ci in images) {
                        ImagesToLoad.Enqueue(ci);
                    }
                }

                SendProgressUpdate();
                foreach (BackgroundWorker worker in this.imageLoadWorkers) {
                    if (!worker.IsBusy)
                        worker.RunWorkerAsync();
                }
            });
        }

        private void CalculateSimilarities(ComparableImage ci) {
            ComparableImage other_image = SafelyGetNextImage(null);
            while(other_image!=null) {
                if (OnlyDifferentSources && ci.Source == other_image.Source) {
                    other_image = SafelyGetNextImage(other_image);
                    continue;
                }

                if (OnlyDifferentFolders && ci.ImagePath.ToLower() == other_image.ImagePath.ToLower()) {
                    other_image = SafelyGetNextImage(other_image);
                    continue;
                }

                if (OnlyDifferentSizes && (ci.ImageHeight == other_image.ImageHeight && ci.ImageWidth== other_image.ImageWidth))
                {
                    other_image = SafelyGetNextImage(other_image);
                    continue;
                }


                if (ci.ImageFile == other_image.ImageFile) { // Same image check
                    other_image = SafelyGetNextImage(other_image);
                    continue;
                }

                if (OnlyChecksum && ci.Checksum!=other_image.Checksum)
                {
                    other_image = SafelyGetNextImage(other_image);
                    continue;
                }

                if (ci.ContainsResultFor(other_image)) {
                    other_image = SafelyGetNextImage(other_image);
                    continue;
                }

                double result = other_image.CompareImage(ci);

                ci.AddResult(other_image, result);
                other_image.AddResult(ci, result);

                other_image = SafelyGetNextImage(other_image);
            }

        }

        private void FindDupes(ComparableImage image) {
            if (image.CurrentDuplicateSet != null)
                return; // Already part of a dupe set

            List<ComparisonResult> similar = image.GetSimilarImages();

            DuplicateImageSet dis = null;

            ComparisonResult cr = image.GetTopSimilarImage();
            if (cr == null) // This just means the comparisons haven't been run yet.
                return;

            if (cr.Result >= Fuzziness) {
                if (cr.Image.CurrentDuplicateSet == null) {
                    dis = new DuplicateImageSet(cr.Image);
                } else if (!cr.Image.CurrentDuplicateSet.ContainsImage(image)) {
                    ComparisonResult first = cr.Image.CurrentDuplicateSet.First<ComparisonResult>().Image.GetComparisonResultForImage(image);
                    if (first == null)
                        return;
                    if (first.Result < Fuzziness)
                        return;
                    dis = cr.Image.CurrentDuplicateSet;
                } else {
                    Console.Out.WriteLine();
                }
            }


            if (dis != null) {
                dis.AddImage(image);

                App.Current.Dispatcher.Invoke((Action)(() => {
                    lock (this) {
                        if (!this.Contains(dis))
                            this.Add(dis);
                    }
                }));
            }

        }

        public void RemoveImage(ComparableImage ci) {
            DuplicateImageSet dis = ci.CurrentDuplicateSet;
            if (dis != null) {
                lock (dis) {
                    dis.RemoveImage(ci);
                    lock (this) {
                        if (this.Contains(dis)&&dis.Count==1) {
                            this.Remove(dis);
                        }
                    }
                }
            }
            foreach(ComparisonResult other in ci.GetSimilarImages()) {
                other.Image.RemoveResultFor(ci);
            }

            lock (images) {
                if (this.images.Contains(ci))
                    this.images.Remove(ci);
            }
            SendProgressUpdate();
        }

        Mutex comparisonMutex = new Mutex();
        public async void recompareImages() {
            await Task.Run(() => {
                comparisonMutex.WaitOne();
                App.Current.Dispatcher.Invoke((Action)(() => {
                    lock (this) {
                        this.Clear();
                    }
                }));
                lock(this.images) {
                    lock(ImagesToCompare) {
                        foreach (ComparableImage ci in this.images) {
                            ci.CurrentDuplicateSet = null;
                            ci.Selected = false;
                            //ci.ClearSimilarImages();
                            if(File.Exists(ci.ImageFile))
                                ImagesToCompare.Enqueue(ci);
                        }
                        this.images.Clear();
                    }
                }
                if (!imageCompareWorkers.IsBusy)
                    imageCompareWorkers.RunWorkerAsync();

                comparisonMutex.ReleaseMutex();
            });
        }

        Mutex thumbnailMutex = new Mutex();
        bool isRegeneratingThumbnails = false, cancelThumbnailing = false;
        public async void reGenerateThumbnails() {
            await Task.Run(() => {
                thumbnailMutex.WaitOne();
                lock (this) {

                    foreach (DuplicateImageSet dis in this) {
                        lock (dis) {

                            foreach (ComparisonResult cr in dis) {
                                lock (cr) {
                                    lock(cr.Image) {
                                        cr.Image.RegenerateThumbnail();
                                    }
                                }
                            }
                        }
                    }
                }
                thumbnailMutex.ReleaseMutex();
            });
        }

        public void PerformAutoSelect() {
            if (selectors.Count == 0)
                throw new Exception("No selectors added");

            foreach (DuplicateImageSet dis in this) {
                PerformAutoSelect(dis);
            }

        }

        public void PerformAutoSelect(DuplicateImageSet set)
        {
            List<ComparableImage> setImages = set.GetImages();
            this.selectors.PerformSelection(setImages);
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
