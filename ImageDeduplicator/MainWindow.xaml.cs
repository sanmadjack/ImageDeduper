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
            imagesList.ItemsSource = comparitor;
        }

        private void setFoldeRButton_Click(object sender, RoutedEventArgs e) {
            if(setDownloadFolder()) {
                comparitor.Clear();
                comparitor.LoadDirectory(new System.IO.DirectoryInfo(Properties.Settings.Default.LastDownloadDir), false);
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

        private void Image_MouseEnter(object sender, MouseEventArgs e) {
            Image img = (Image)sender;
            ComparableImage ci = (ComparableImage)img.DataContext;
             ImageSource imageSource = new BitmapImage(new Uri(ci.ImageFile.FullName));
            previewImage.Source = imageSource;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e) {

        }
    }
}
