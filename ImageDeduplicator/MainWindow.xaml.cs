using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls.Ribbon;
using System.Collections.ObjectModel;
using System.IO;
using ImageDeduplicator.ImageSources;
using ImageDeduplicator.SelectionCriteria;

namespace ImageDeduplicator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow {


        Comparitor comparitor = new Comparitor();
        public MainWindow() {
            InitializeComponent();
            this.DataContext = comparitor;
            imagesList.DataContext = comparitor;
            loadingProgress.DataContext = comparitor;
            thumbnailZoomSlider.DataContext = comparitor;
            simliaritySlider.DataContext = comparitor;
            similarityLabel.DataContext = comparitor;
            selectorList.DataContext = comparitor.selectors;
            sourcesList.DataContext = comparitor.Sources;
        }

        private void loadImages_Click(object sender, RoutedEventArgs e) {
            Control cont = (Control)sender;
            String arg = "";
            switch (cont.Tag.ToString()) {
                case "folder":
                    if (setDownloadFolder()) {
                        arg = Properties.Settings.Default.LastDownloadDir;
                    } else {
                        return;
                    }
                    break;
                case "file_list":
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.DefaultExt = ".txt|.csv";
                    dlg.Filter = "Text File (*.txt)|*.txt|CSV File (*.csv)|*.csv";

                    Nullable<bool> result = dlg.ShowDialog();

                    if (result == true) {
                        arg = dlg.FileName;
                    } else {
                        return;
                    }
                    break;
            }
            loadImages(cont.Tag.ToString(), arg);
        }


        private void loadImages(String method, String arg = null) {
            AImageSource source;
            switch (method) {
                case "folder":
                    source = new FolderImageSource(arg);
                    break;
                case "file_list":
                    source = new TextFileImageSource(arg);
                    break;
                case "database":
                    DatabaseSourceEntry entry = new DatabaseSourceEntry();
                    if (!entry.ShowDialog().Value)
                        return;
                    source = entry.getImageSource();
                    break;
                case "shimmie":
                    ShimmieSourceEntry shimmieEntry = new ShimmieSourceEntry();
                    if (!shimmieEntry.ShowDialog().Value)
                        return;
                    source = shimmieEntry.getImageSource();
                    break;
                default:
                    return;
            }
            MessageBoxResult result = MessageBox.Show(this, "Do you want to clear the current comparison?", "Clear comparison", MessageBoxButton.YesNoCancel);
            switch(result) {
                case MessageBoxResult.Cancel:
                    return;
                case MessageBoxResult.Yes:
                    comparitor.Reset();
                    break;
            }
            comparitor.LoadFilesAsync(source);

        }


        private string ChooseFolder(string start_dir) { 
            if(!String.IsNullOrWhiteSpace(start_dir)) {
                DirectoryInfo di = new DirectoryInfo(start_dir);
                while(!di.Exists) {
                    di = di.Parent;
                }
                start_dir = di.FullName;
            }

            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "Select folder";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = start_dir;
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = start_dir;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog(this) == CommonFileDialogResult.Cancel) {
                return null;
            }
            return dlg.FileName;
        }

        private bool setDownloadFolder() {
            string selected_dir = ChooseFolder(Properties.Settings.Default.LastDownloadDir);
            if (selected_dir == null)
                return false;

            Properties.Settings.Default.LastDownloadDir = selected_dir;
            Properties.Settings.Default.Save();

            return true;
        }

        private DuplicateImageSet currentSet = null;
        private void Image_MouseEnter(object sender, MouseEventArgs e) {
            Image img = (Image)sender;
            ComparisonResult cr = (ComparisonResult)img.DataContext;
            comparisonSet.AddImage(cr);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Image img = (Image)sender;
                ComparisonResult cr = (ComparisonResult)img.DataContext;
                cr.Image.Selected = !cr.Image.Selected;
            }
            if (e.MiddleButton == MouseButtonState.Pressed) {
                Image img = (Image)sender;
                ComparisonResult cr = (ComparisonResult)img.DataContext;
                comparisonSet.ToggleImage(cr);
            }
        }



        private List<ComparableImage> GatherSelectedFiles() {
            List<ComparableImage> output = new List<ComparableImage>();
            foreach(DuplicateImageSet dis in this.comparitor) {
                foreach(ComparableImage ci in dis.GetImages()) {
                    if (ci.Selected)
                        output.Add(ci);
                }
            }
            return output;
        }
        private void deleteButton_Click(object sender, RoutedEventArgs e) {
            List<ComparableImage> files = GatherSelectedFiles();
            foreach(ComparableImage ci in files) {
                try {
                    ci.Delete();
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message);
                }
                if (!File.Exists(ci.ImageFile))
                    this.comparitor.RemoveImage(ci);
            }
        }

        private void mergeButton_Click(object sender, RoutedEventArgs e) {
            List<ComparableImage> files = GatherSelectedFiles();
            foreach (ComparableImage ci in files) {
                DuplicateImageSet dis = ci.CurrentDuplicateSet;
                if(dis.GetUnselectedImages().Count !=1) {
                    MessageBox.Show(this, "Only one image must be unselected in a set in order to merge");
                    continue;
                }

                try {
                    ComparableImage target = dis.GetUnselectedImages()[0];
                    ComparableImage mergedImage = ci.Merge(target);
                    dis.ReplaceImage(target, mergedImage);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
                if (!File.Exists(ci.ImageFile))
                    this.comparitor.RemoveImage(ci);
            }
        }


        private void moveButton_Click(object sender, RoutedEventArgs e) {
            string selected_dir = ChooseFolder(Properties.Settings.Default.LastMoveDir);
            if (selected_dir == null)
                return;

            Properties.Settings.Default.LastMoveDir = selected_dir;
            Properties.Settings.Default.Save();

            List<ComparableImage> files = GatherSelectedFiles();
            foreach (ComparableImage ci in files) {
                try {
                    FileInfo fi = new FileInfo(ci.ImageFile);
                    string new_path = System.IO.Path.Combine(selected_dir, fi.Name);

                    if (File.Exists(new_path))
                        MessageBox.Show(fi.Name + " already exists in the destination");

                    fi.MoveTo(new_path);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }
                if(!File.Exists(ci.ImageFile))
                    this.comparitor.RemoveImage(ci);
            }

            return;
        }

        private void thumbnailZoomSlider_MouseUp(object sender, MouseButtonEventArgs e) {
            comparitor.reGenerateThumbnails();
        }

        private void simliaritySlider_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            comparitor.recompareImages();
        }

        private void addSelectorButton_Click(object sender, RoutedEventArgs e) {
            addSelectorButton.ContextMenu.IsOpen = true;
        }

        private void removeSelectorButton_Click(object sender, RoutedEventArgs e) {
            System.Collections.IList crit = selectorList.SelectedItems;
            List<ASelectionCriteria> toRemove = new List<ASelectionCriteria>();
            foreach(object obj in crit) {
                toRemove.Add((ASelectionCriteria)obj);
            }
            foreach(ASelectionCriteria sc in toRemove) {
                this.comparitor.selectors.Remove(sc);
            }
            Properties.Settings.Default.Save();
        }

        private void upSelectorButton_Click(object sender, RoutedEventArgs e) {
            System.Collections.IList crit = selectorList.SelectedItems;
            if (crit.Count == 0)
                return;

            Object last = crit[crit.Count - 1];
            Object first = crit[crit.Count - 1];
            int last_position = this.comparitor.selectors.IndexOf((ASelectionCriteria)last);
            int first_position = this.comparitor.selectors.IndexOf((ASelectionCriteria)first);

            if (first_position==0)
                return;

            ASelectionCriteria prev = this.comparitor.selectors[first_position-1];
            this.comparitor.selectors.RemoveAt(first_position - 1);
            this.comparitor.selectors.Insert(last_position, prev);


            Properties.Settings.Default.Save();
        }

        private void downSelectorButton_Click(object sender, RoutedEventArgs e) {
            System.Collections.IList crit = selectorList.SelectedItems;
            if (crit.Count == 0)
                return;

            Object last = crit[crit.Count - 1];
            Object first = crit[crit.Count - 1];
            int last_position = this.comparitor.selectors.IndexOf((ASelectionCriteria)last);
            int first_position = this.comparitor.selectors.IndexOf((ASelectionCriteria)first);

            if (this.comparitor.selectors.Count == last_position + 1)
                return;

            ASelectionCriteria next = this.comparitor.selectors[last_position + 1];
            this.comparitor.selectors.RemoveAt(last_position + 1);
            this.comparitor.selectors.Insert(first_position, next);

            Properties.Settings.Default.Save();
        }

        private void autoSelectButton_Click(object sender, RoutedEventArgs e) {
            try {
                comparitor.PerformAutoSelect();
            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        #region Selection criteria menu item event handlers

        private void smallerPixelCountSelectorMenuItem_Click(object sender, RoutedEventArgs e) {
            comparitor.selectors.Add(new SmallerPixelCountSelectionCriteria());
            Properties.Settings.Default.Save();
        }

        private void smallerFileSizeSelectorMenuItem_Click(object sender, RoutedEventArgs e) {
            comparitor.selectors.Add(new SmallerFileSizeSelectionCriteria());
            Properties.Settings.Default.Save();
        }


        private void fileNameRegexSelectorMenuItem_Click(object sender, RoutedEventArgs e) {
            TextInputDialog dlg = new TextInputDialog("Please provide the regex", true);
            if (!dlg.ShowDialog().Value)
                return;
            comparitor.selectors.Add(new FileNameRegexSelectionCriteria(dlg.GetInput(), dlg.GetInvert()));
            Properties.Settings.Default.Save();
        }

        private void parentDirectorySelectorMenuItem_Click(object sender, RoutedEventArgs e) {
            string selected_dir = ChooseFolder(Properties.Settings.Default.LastMoveDir);
            if (selected_dir == null)
                return;

            Properties.Settings.Default.LastMoveDir = selected_dir;

            comparitor.selectors.Add(new ParentDirectorySelectionCriteria(selected_dir));
            Properties.Settings.Default.Save();
        }

        private void pathRegexSelectorMenuItem_Click(object sender, RoutedEventArgs e) {
            TextInputDialog dlg = new TextInputDialog("Please provide the regex", true);
            if (!dlg.ShowDialog().Value)
                return;
            comparitor.selectors.Add(new PathRegexSelectionCriteria(dlg.GetInput(), dlg.GetInvert()));
            Properties.Settings.Default.Save();

        }

        private void lossyFIleFormatSelectorMenuItem_Click(object sender, RoutedEventArgs e) {
            comparitor.selectors.Add(new LossyFileFormatSelectionCriteria());
            Properties.Settings.Default.Save();
        }
        #endregion

        bool adjusting_scroll = false;
        private void setScrollPositions(double x, double y, ScrollViewer sv = null) {
            if (adjusting_scroll)
                return;

            adjusting_scroll = true;
            try {
                foreach (ScrollViewer view in Utilities.FindVisualChildren<ScrollViewer>(imageViewer)) {
                    if (view == sv)
                        continue;

                    if (x == 0 || Double.IsNaN(x)) {
                        view.ScrollToHorizontalOffset(0);
                    } else {
                        view.ScrollToHorizontalOffset(view.ScrollableWidth * x);
                    }
                    if (y == 0 || Double.IsNaN(y)) {
                        view.ScrollToVerticalOffset(0);
                    } else {
                        view.ScrollToVerticalOffset(view.ScrollableHeight * y);
                    }
                }
            } finally {
                adjusting_scroll = false;
            }
        }
        private void imageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            ScrollViewer sv = (ScrollViewer)sender;
            double x = sv.HorizontalOffset / sv.ScrollableWidth; 
            double y = sv.VerticalOffset/ sv.ScrollableHeight; 

            setScrollPositions(x, y, sv);
        }


        private void previewImage_MouseWheel(object sender, MouseWheelEventArgs e) {
            ScrollViewer sv = Utilities.FindParent<ScrollViewer>((Image)sender);
            double x = sv.HorizontalOffset / sv.ScrollableWidth; 
            double y = sv.VerticalOffset / sv.ScrollableHeight; 

            int zoom = ComparisonResult.ZoomLevel;
            zoom += e.Delta/20;
            foreach (Image img in Utilities.FindVisualChildren<Image>(imageViewer)) {
                ComparisonResult cr = (ComparisonResult)img.DataContext;
                cr.Zoom = zoom;
            }

            setScrollPositions(x, y);

            e.Handled = true;
        }



        private void previewImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            if (!comparisonSet.ScaleImage) {
                Image img = (Image)sender;
                ScrollViewer sv = Utilities.FindParent<ScrollViewer>((Image)sender);
                Point p = e.GetPosition(img);


                double x = p.X / img.Width;
                double y = p.Y / img.Height;

                ComparisonResult.ZoomLevel = 100;
                comparisonSet.ScaleImage = !comparisonSet.ScaleImage;
                setScrollPositions(x, y);

            } else {
                ComparisonResult.ZoomLevel = 100;
                comparisonSet.ScaleImage = !comparisonSet.ScaleImage;
            }




        }

        private void clearComparisonButton_Click(object sender, RoutedEventArgs e) {
            lock(this.comparisonSet) {
                this.comparisonSet.Clear();
            }
        }

        private void RibbonWindow_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                if (files.Length > 0) {
                    FileAttributes attr = File.GetAttributes(files[0]);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                        loadImages("folder",files[0]);
                    } else {
                        try {
                            FileInfo fi = new FileInfo(files[0]);
                            loadImages("file_list", fi.FullName);
                        } catch (Exception ex) {
                            Console.Out.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        private void MenuItem_MouseDown(object sender, MouseButtonEventArgs e) {
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            Image img = (Image)getContextMenuParent(sender);
            ComparisonResult cr = (ComparisonResult)img.DataContext;
            cr.Image.CurrentDuplicateSet.RemoveImage(cr.Image);
        }

        private object getContextMenuParent(object sender) {
            MenuItem menuItem = sender as MenuItem;
                ContextMenu parentContextMenu = menuItem.CommandParameter as ContextMenu;
                    return parentContextMenu.PlacementTarget;
        }
    }
}
