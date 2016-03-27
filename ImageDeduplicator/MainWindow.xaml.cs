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


namespace ImageDeduplicator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow {


        //Comparitor comparitor = new Comparitor();

        public MainWindow() {
            InitializeComponent();
            loadingProgress.DataContext = comparitor;
        }

        private void setFoldeRButton_Click(object sender, RoutedEventArgs e) {
            if (setDownloadFolder()) {
                comparitor.Clear();
                comparitor.LoadDirectoryAsync(Properties.Settings.Default.LastDownloadDir, false);
            }
        }

        private bool setDownloadFolder() {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "Select download folder";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Properties.Settings.Default.LastDownloadDir;
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = Properties.Settings.Default.LastDownloadDir;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog(this) == CommonFileDialogResult.Cancel) {
                return false;
            }
            string selected_dir = dlg.FileName;
            Properties.Settings.Default.LastDownloadDir = selected_dir;
            Properties.Settings.Default.Save();
            return true;
        }

        private DuplicateImageSet currentSet = null;
        private void Image_MouseEnter(object sender, MouseEventArgs e) {
            Image img = (Image)sender;
            ComparableImage ci = (ComparableImage)img.DataContext;
            if (ci.CurrentDuplicateSet == currentSet) {
                comparisonSet.SetTempImage(ci);
            } else {
                comparisonSet.Clear();
                comparisonSet.SetTempImage(ci);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Image img = (Image)sender;
                ComparableImage ci = (ComparableImage)img.DataContext;
                ci.Selected = !ci.Selected;
            }
            //if(e.RightButton== MouseButtonState.Pressed) {
            //    Image img = (Image)sender;
            //    ComparableImage ci = (ComparableImage)img.DataContext;
            //    comparisonSet.ToggleCurrentImageSave(ci);
            //}
        }

        private void autoSelectButton_Click(object sender, RoutedEventArgs e) {
            foreach(DuplicateImageSet dis in this.comparitor) {
                bool already_selected = false;
                foreach(ComparableImage ci in dis) {
                    if (ci.Selected) {
                        already_selected = true;
                        break;
                    }

                    if(higherResCheck.IsChecked.Value) {

                    }
                }
                if (already_selected)
                    continue;


            }
        }

        private List<ComparableImage> GatherSelectedFiles() {
            List<ComparableImage> output = new List<ComparableImage>();
            foreach(DuplicateImageSet dis in this.comparitor) {
                foreach(ComparableImage ci in dis) {
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
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(ci.ImageFile, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    this.comparitor.RemoveImage(ci);
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void moveButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
